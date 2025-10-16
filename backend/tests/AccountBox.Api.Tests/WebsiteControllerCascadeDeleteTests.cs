using System.Net;
using System.Net.Http.Json;
using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Account;
using AccountBox.Core.Models.Vault;
using AccountBox.Core.Models.Website;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccountBox.Api.Tests;

/// <summary>
/// WebsiteController 级联删除集成测试
/// 测试网站删除时的级联保护逻辑（活跃账号检查、回收站账号确认）
/// </summary>
[Collection("VaultService Tests")]
public class WebsiteControllerCascadeDeleteTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName;
    private readonly string _sessionId;

    public WebsiteControllerCascadeDeleteTests()
    {
        // 清除静态状态
        VaultService.ResetFailedAttemptsForTesting();

        // 为每个测试创建唯一的数据库名称
        _databaseName = $"TestDb_CascadeDelete_{Guid.NewGuid()}";

        // 创建测试服务器
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            var dbName = _databaseName;

            builder.ConfigureServices(services =>
            {
                // 移除原有的 DbContext
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AccountBoxDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(AccountBoxDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // 创建新的 ServiceProvider 用于 DbContext
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // 添加 InMemory 数据库
                services.AddDbContext<AccountBoxDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName)
                           .UseInternalServiceProvider(serviceProvider)
                           .EnableSensitiveDataLogging();
                });
            });
        });

        _client = _factory.CreateClient();

        // 初始化 vault 并获取会话
        var initResponse = _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = "TestPassword123!" }).Result;
        var initResult = initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>().Result;
        _sessionId = initResult!.Data!.SessionId;
    }

    [Fact]
    public async Task DeleteWebsite_WithActiveAccounts_ShouldReturn409Conflict()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // 创建活跃账号
        var createAccountRequest = new CreateAccountRequest
        {
            WebsiteId = websiteId,
            Username = "activeuser",
            Password = "password123"
        };

        var createAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createAccountRequest)
        };
        createAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        await _client.SendAsync(createAccountMsg);

        // Act - 尝试删除网站
        var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}");
        deleteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("ACTIVE_ACCOUNTS_EXIST");
        result.Error.Message.Should().Contain("active account");
        result.Error.Details.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteWebsite_WithDeletedAccountsOnly_NotConfirmed_ShouldReturn409Conflict()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // 创建账号并软删除
        var createAccountRequest = new CreateAccountRequest
        {
            WebsiteId = websiteId,
            Username = "deleteduser",
            Password = "password123"
        };

        var createAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createAccountRequest)
        };
        createAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createAccountResponse = await _client.SendAsync(createAccountMsg);
        var createAccountResult = await createAccountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var accountId = createAccountResult!.Data!.Id;

        // 软删除账号
        var deleteAccountMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/accounts/{accountId}");
        deleteAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        await _client.SendAsync(deleteAccountMsg);

        // Act - 尝试删除网站（未确认）
        var deleteWebsiteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}");
        deleteWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteWebsiteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("CONFIRMATION_REQUIRED");
        result.Error.Message.Should().Contain("deleted account");
        result.Error.Details.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteWebsite_WithDeletedAccountsOnly_Confirmed_ShouldSucceed()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // 创建账号并软删除
        var createAccountRequest = new CreateAccountRequest
        {
            WebsiteId = websiteId,
            Username = "deleteduser",
            Password = "password123"
        };

        var createAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createAccountRequest)
        };
        createAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createAccountResponse = await _client.SendAsync(createAccountMsg);
        var createAccountResult = await createAccountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var accountId = createAccountResult!.Data!.Id;

        // 软删除账号
        var deleteAccountMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/accounts/{accountId}");
        deleteAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        await _client.SendAsync(deleteAccountMsg);

        // Act - 删除网站（已确认）
        var deleteWebsiteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}?confirmed=true");
        deleteWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteWebsiteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // 验证网站已删除
        var getWebsiteMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/websites/{websiteId}");
        getWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var getWebsiteResponse = await _client.SendAsync(getWebsiteMsg);
        getWebsiteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteWebsite_WithNoAccounts_ShouldSucceed()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // Act - 删除网站
        var deleteWebsiteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}");
        deleteWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteWebsiteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // 验证网站已删除
        var getWebsiteMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/websites/{websiteId}");
        getWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var getWebsiteResponse = await _client.SendAsync(getWebsiteMsg);
        getWebsiteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteWebsite_WithMixedAccounts_ShouldReturn409ForActiveAccounts()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // 创建活跃账号
        var createActiveAccountRequest = new CreateAccountRequest
        {
            WebsiteId = websiteId,
            Username = "activeuser",
            Password = "password123"
        };

        var createActiveAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createActiveAccountRequest)
        };
        createActiveAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        await _client.SendAsync(createActiveAccountMsg);

        // 创建并软删除另一个账号
        var createDeletedAccountRequest = new CreateAccountRequest
        {
            WebsiteId = websiteId,
            Username = "deleteduser",
            Password = "password123"
        };

        var createDeletedAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createDeletedAccountRequest)
        };
        createDeletedAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createDeletedAccountResponse = await _client.SendAsync(createDeletedAccountMsg);
        var createDeletedAccountResult = await createDeletedAccountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var deletedAccountId = createDeletedAccountResult!.Data!.Id;

        // 软删除第二个账号
        var deleteAccountMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/accounts/{deletedAccountId}");
        deleteAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        await _client.SendAsync(deleteAccountMsg);

        // Act - 尝试删除网站
        var deleteWebsiteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}");
        deleteWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteWebsiteMsg);

        // Assert - 应该返回活跃账号错误（优先级更高）
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("ACTIVE_ACCOUNTS_EXIST");
    }

    [Fact]
    public async Task DeleteWebsite_WithActiveAccounts_Confirmed_ShouldStillFail()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // 创建活跃账号
        var createAccountRequest = new CreateAccountRequest
        {
            WebsiteId = websiteId,
            Username = "activeuser",
            Password = "password123"
        };

        var createAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createAccountRequest)
        };
        createAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
        await _client.SendAsync(createAccountMsg);

        // Act - 尝试删除网站（即使已确认）
        var deleteWebsiteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}?confirmed=true");
        deleteWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteWebsiteMsg);

        // Assert - 即使确认了，有活跃账号时也应该失败
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("ACTIVE_ACCOUNTS_EXIST");
    }

    [Fact]
    public async Task DeleteWebsite_WithMultipleDeletedAccounts_Confirmed_ShouldSucceed()
    {
        // Arrange - 创建网站
        var createWebsiteRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createWebsiteMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsiteRequest)
        };
        createWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsiteResponse = await _client.SendAsync(createWebsiteMsg);
        var createWebsiteResult = await createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createWebsiteResult!.Data!.Id;

        // 创建并删除多个账号
        for (int i = 1; i <= 3; i++)
        {
            var createAccountRequest = new CreateAccountRequest
            {
                WebsiteId = websiteId,
                Username = $"deleteduser{i}",
                Password = "password123"
            };

            var createAccountMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
            {
                Content = JsonContent.Create(createAccountRequest)
            };
            createAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
            var createAccountResponse = await _client.SendAsync(createAccountMsg);
            var createAccountResult = await createAccountResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
            var accountId = createAccountResult!.Data!.Id;

            // 软删除账号
            var deleteAccountMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/accounts/{accountId}");
            deleteAccountMsg.Headers.Add("X-Vault-Session", _sessionId);
            await _client.SendAsync(deleteAccountMsg);
        }

        // Act - 删除网站（已确认）
        var deleteWebsiteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}?confirmed=true");
        deleteWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteWebsiteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // 验证网站已删除
        var getWebsiteMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/websites/{websiteId}");
        getWebsiteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var getWebsiteResponse = await _client.SendAsync(getWebsiteMsg);
        getWebsiteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        // 清理测试后的静态状态
        VaultService.ResetFailedAttemptsForTesting();

        _client.Dispose();
        _factory.Dispose();
    }
}
