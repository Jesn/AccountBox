using System.Net;
using System.Net.Http.Json;
using AccountBox.Api.Services;
using AccountBox.Core.Models;
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
/// WebsiteController HTTP API 集成测试
/// 测试完整的 HTTP 请求/响应流程
/// </summary>
[Collection("VaultService Tests")]
public class WebsiteControllerApiTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName;
    private readonly string _sessionId;

    public WebsiteControllerApiTests()
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
    }

    [Fact]
    public async Task GetPaged_WithNoWebsites_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/websites?pageNumber=1&pageSize=10");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<WebsiteResponse>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201AndWebsite()
    {
        // Arrange
        var createRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example Website",
            Tags = "test,demo"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createRequest)
        };
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().BeGreaterThan(0);
        result.Data.Domain.Should().Be("example.com");
        result.Data.DisplayName.Should().Be("Example Website");
        result.Data.Tags.Should().Be("test,demo");
        result.Data.ActiveAccountCount.Should().Be(0);
        result.Data.DeletedAccountCount.Should().Be(0);
    }

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200AndWebsite()
    {
        // Arrange - Create a website first
        var createRequest = new CreateWebsiteRequest
        {
            Domain = "github.com",
            DisplayName = "GitHub"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createResult!.Data!.Id;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/websites/{websiteId}");
        request.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(websiteId);
        result.Data.Domain.Should().Be("github.com");
        result.Data.DisplayName.Should().Be("GitHub");
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/websites/999");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturn200AndUpdatedWebsite()
    {
        // Arrange - Create a website first
        var createRequest = new CreateWebsiteRequest
        {
            Domain = "example.com",
            DisplayName = "Example"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createResult!.Data!.Id;

        // Act - Update the website
        var updateRequest = new UpdateWebsiteRequest
        {
            Domain = "example.org",
            DisplayName = "Example Organization",
            Tags = "updated"
        };

        var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/websites/{websiteId}")
        {
            Content = JsonContent.Create(updateRequest)
        };
        updateMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(websiteId);
        result.Data.Domain.Should().Be("example.org");
        result.Data.DisplayName.Should().Be("Example Organization");
        result.Data.Tags.Should().Be("updated");
    }

    [Fact]
    public async Task Update_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var updateRequest = new UpdateWebsiteRequest
        {
            Domain = "nonexist.com",
            DisplayName = "Non-exist"
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/websites/999")
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
    public async Task Delete_WithExistingId_ShouldReturn200()
    {
        // Arrange - Create a website first
        var createRequest = new CreateWebsiteRequest
        {
            Domain = "todelete.com",
            DisplayName = "To Delete"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createResult!.Data!.Id;

        // Act - Delete the website
        var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/websites/{websiteId}");
        deleteMsg.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(deleteMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();

        // Verify it's deleted - GetById should return 404
        var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/websites/{websiteId}");
        getMsg.Headers.Add("X-Vault-Session", _sessionId);
        var getResponse = await _client.SendAsync(getMsg);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/websites/999");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountCount_WithExistingId_ShouldReturn200AndCount()
    {
        // Arrange - Create a website first
        var createRequest = new CreateWebsiteRequest
        {
            Domain = "test.com",
            DisplayName = "Test"
        };

        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
        {
            Content = JsonContent.Create(createRequest)
        };
        createMsg.Headers.Add("X-Vault-Session", _sessionId);
        var createResponse = await _client.SendAsync(createMsg);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<WebsiteResponse>>();
        var websiteId = createResult!.Data!.Id;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/websites/{websiteId}/accounts/count");
        request.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AccountCountResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ActiveCount.Should().Be(0);
        result.Data.DeletedCount.Should().Be(0);
        result.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAccountCount_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/websites/999/accounts/count");
        request.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaged_WithMultipleWebsites_ShouldReturnPagedResult()
    {
        // Arrange - Create multiple websites
        for (int i = 1; i <= 15; i++)
        {
            var createRequest = new CreateWebsiteRequest
            {
                Domain = $"site{i}.com",
                DisplayName = $"Site {i}"
            };

            var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/websites")
            {
                Content = JsonContent.Create(createRequest)
            };
            createMsg.Headers.Add("X-Vault-Session", _sessionId);
            await _client.SendAsync(createMsg);
        }

        // Act - Get first page
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/websites?pageNumber=1&pageSize=10");
        request.Headers.Add("X-Vault-Session", _sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<WebsiteResponse>>>();
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
    public async Task WebsiteOperations_WithoutSession_ShouldReturn401()
    {
        // Act & Assert - GET without session
        var getResponse = await _client.GetAsync("/api/websites");
        getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - POST without session
        var postResponse = await _client.PostAsJsonAsync("/api/websites", new CreateWebsiteRequest { Domain = "test.com" });
        postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - PUT without session
        var putResponse = await _client.PutAsJsonAsync("/api/websites/1", new UpdateWebsiteRequest { Domain = "test.com" });
        putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Act & Assert - DELETE without session
        var deleteResponse = await _client.DeleteAsync("/api/websites/1");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public void Dispose()
    {
        // 清理测试后的静态状态
        VaultService.ResetFailedAttemptsForTesting();

        _client.Dispose();
        _factory.Dispose();
    }
}
