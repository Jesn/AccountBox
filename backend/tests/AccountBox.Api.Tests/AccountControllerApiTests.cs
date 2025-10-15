using System.Net;
using System.Net.Http.Json;
using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Account;
using AccountBox.Core.Models.Vault;
using AccountBox.Core.Models.Website;
using AccountBox.Data.DbContext;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccountBox.Api.Tests;

/// <summary>
/// AccountController HTTP API 集成测试
/// 测试完整的 HTTP 请求/响应流程
/// </summary>
[Collection("VaultService Tests")]
public class AccountControllerApiTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName;
    private readonly string _sessionId;
    private readonly int _websiteId;

    public AccountControllerApiTests()
    {
        // 清除静态状态
        VaultService.ResetFailedAttemptsForTesting();

        // 为每个测试创建唯一的数据库名称(但在整个测试过程中保持不变)
        _databaseName = $"TestDb_{Guid.NewGuid()}";

        // 为每个测试创建独立的测试服务器
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            var dbName = _databaseName; // Capture in closure

            builder.ConfigureServices(services =>
            {
                // 移除原有的 DbContext 和 DbContextOptions
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

                // 添加 InMemory 数据库 - 使用固定的数据库名称
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

        // 创建一个网站用于测试
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
        var createWebsiteResponse = _client.SendAsync(createWebsiteMsg).Result;
        var createWebsiteResult = createWebsiteResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>().Result;
        _websiteId = createWebsiteResult!.Data!.Id;
    }

    [Fact]
    public async Task GetPaged_WithNoAccounts_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/accounts?pageNumber=1&pageSize=10");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AccountResponse>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201AndAccount()
    {
        // Arrange
        var createRequest = new CreateAccountRequest
        {
            WebsiteId = _websiteId,
            Username = "testuser",
            Password = "SecurePassword123!",
            Notes = "Test notes",
            Tags = "test,demo"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createRequest)
        };
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Debug: Print response if not successful
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content: {content}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().BeGreaterThan(0);
        result.Data.WebsiteId.Should().Be(_websiteId);
        result.Data.Username.Should().Be("testuser");
        result.Data.Password.Should().Be("SecurePassword123!"); // Decrypted password
        result.Data.Notes.Should().Be("Test notes");
        result.Data.Tags.Should().Be("test,demo");
        result.Data.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200AndAccount()
    {
        // Arrange - Create an account first
        var createRequest = new CreateAccountRequest
        {
            WebsiteId = _websiteId,
            Username = "user1",
            Password = "Password123!"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var accountId = createResult!.Data!.Id;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/accounts/{accountId}");
        request.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(accountId);
        result.Data.Username.Should().Be("user1");
        result.Data.Password.Should().Be("Password123!");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/accounts/999");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturn200AndUpdatedAccount()
    {
        // Arrange - Create an account first
        var createRequest = new CreateAccountRequest
        {
            WebsiteId = _websiteId,
            Username = "olduser",
            Password = "OldPassword123!"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var accountId = createResult!.Data!.Id;

        // Act - Update the account
        var updateRequest = new UpdateAccountRequest
        {
            Username = "newuser",
            Password = "NewPassword456!",
            Notes = "Updated notes",
            Tags = "updated"
        };

        var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/accounts/{accountId}")
        {
            Content = JsonContent.Create(updateRequest)
        };
        updateMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(accountId);
        result.Data.Username.Should().Be("newuser");
        result.Data.Password.Should().Be("NewPassword456!");
        result.Data.Notes.Should().Be("Updated notes");
        result.Data.Tags.Should().Be("updated");
    }

    [Fact]
    public async Task Update_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var updateRequest = new UpdateAccountRequest
        {
            Username = "nonexist",
            Password = "Password123!"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/accounts/999")
        {
            Content = JsonContent.Create(updateRequest)
        };
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithExistingId_ShouldReturn200AndSoftDelete()
    {
        // Arrange - Create an account first
        var createRequest = new CreateAccountRequest
        {
            WebsiteId = _websiteId,
            Username = "todelete",
            Password = "Password123!"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var accountId = createResult!.Data!.Id;

        // Act - Delete the account (soft delete)
        var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/accounts/{accountId}");
        deleteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Verify it's soft deleted - GetById should still return 200 but IsDeleted=true
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/accounts/{accountId}");
        getMsg.Headers.Add("X-Vault-Session", _sessionId);
        var getResponse = await _client.SendAsync(getMsg);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        getResult!.Data!.IsDeleted.Should().BeTrue();
        getResult.Data.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/accounts/999");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaged_WithWebsiteIdFilter_ShouldReturnFilteredAccounts()
    {
        // Arrange - Create another website
        var createWebsite2Request = new CreateWebsiteRequest
        {
            Domain = "another.com",
            DisplayName = "Another"
        };
        var createWebsite2Msg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createWebsite2Request)
        };
        createWebsite2Msg.Headers.Add("X-Vault-Session", _sessionId);
        var createWebsite2Response = await _client.SendAsync(createWebsite2Msg);
        var createWebsite2Result = await createWebsite2Response.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var website2Id = createWebsite2Result!.Data!.Id;

        // Create accounts for both websites
        for (int i = 1; i <= 3; i++)
        {
            var createRequest = new CreateAccountRequest
            {
                WebsiteId = _websiteId,
                Username = $"user{i}",
                Password = "Password123!"
            };

            var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
            {
                Content = JsonContent.Create(createRequest)
            };
            createMsg.Headers.Add("X-Vault-Session", _sessionId);
            await _client.SendAsync(createMsg);
        }

        for (int i = 4; i <= 5; i++)
        {
            var createRequest = new CreateAccountRequest
            {
                WebsiteId = website2Id,
                Username = $"user{i}",
                Password = "Password123!"
            };

            var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
            {
                Content = JsonContent.Create(createRequest)
            };
            createMsg.Headers.Add("X-Vault-Session", _sessionId);
            await _client.SendAsync(createMsg);
        }

        // Act - Get accounts for first website only
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/accounts?websiteId={_websiteId}");
        request.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AccountResponse>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.TotalCount.Should().Be(3);
        result.Data.Items.Should().AllSatisfy(a => a.WebsiteId.Should().Be(_websiteId));
    }

    [Fact]
    public async Task GetPaged_WithMultipleAccounts_ShouldReturnPagedResult()
    {
        // Arrange - Create multiple accounts
        for (int i = 1; i <= 15; i++)
        {
            var createRequest = new CreateAccountRequest
            {
                WebsiteId = _websiteId,
                Username = $"user{i}",
                Password = $"Password{i}!"
            };

            var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
            {
                Content = JsonContent.Create(createRequest)
            };
            createMsg.Headers.Add("X-Vault-Session", _sessionId);
            await _client.SendAsync(createMsg);
        }

        // Act - Get first page
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/accounts?pageNumber=1&pageSize=10");
        request.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<AccountResponse>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(15);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task AccountOperations_WithoutSession_ShouldReturn401()
    {
        // Act & Assert - GET without session
        var getResponse = await _client.GetAsync("/api/accounts");
        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - POST without session
        var postResponse = await _client.PostAsJsonAsync("/api/accounts", new CreateAccountRequest
        {
            WebsiteId = _websiteId,
            Username = "test",
            Password = "test"
        });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - PUT without session
        var putResponse = await _client.PutAsJsonAsync("/api/accounts/1", new UpdateAccountRequest
        {
            Username = "test",
            Password = "test"
        });
        putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - DELETE without session
        var deleteResponse = await _client.DeleteAsync("/api/accounts/1");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PasswordEncryption_ShouldWorkCorrectly()
    {
        // Arrange - Create an account with a password
        var originalPassword = "SuperSecretPassword123!";
        var createRequest = new CreateAccountRequest
        {
            WebsiteId = _websiteId,
            Username = "encryptiontest",
            Password = originalPassword
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/accounts")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();
        var accountId = createResult!.Data!.Id;

        // Act - Retrieve the account
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/accounts/{accountId}");
        getMsg.Headers.Add("X-Vault-Session", _sessionId);
        var getResponse = await _client.SendAsync(getMsg);
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<AccountResponse>>();

        // Assert - Password should be decrypted correctly
        getResult!.Data!.Password.Should().Be(originalPassword);
    }

    public void Dispose()
    {
        // 清理测试后的静态状态
        VaultService.ResetFailedAttemptsForTesting();

        _client.Dispose();
        _factory.Dispose();
    }
}
