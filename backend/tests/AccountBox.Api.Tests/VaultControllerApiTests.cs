using System.Net;
using System.Net.Http.Json;
using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Vault;
using AccountBox.Data.DbContext;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccountBox.Api.Tests;

/// <summary>
/// VaultController HTTP API 集成测试
/// 测试完整的 HTTP 请求/响应流程
/// </summary>
[Collection("VaultService Tests")]
public class VaultControllerApiTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName;

    public VaultControllerApiTests()
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
                // 这样 InMemory 和 Sqlite 提供程序不会冲突
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
    }

    [Fact]
    public async Task GetStatus_WhenNotInitialized_ShouldReturnNotInitialized()
    {
        // Act
        var response = await _client.GetAsync("/api/vault/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VaultStatusResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.IsInitialized.Should().BeFalse();
        result.Data.IsUnlocked.Should().BeFalse();
    }

    [Fact]
    public async Task Initialize_WithValidPassword_ShouldReturn200AndSession()
    {
        // Arrange
        var request = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vault/initialize", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionId.Should().NotBeNullOrEmpty();
        result.Data.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Initialize_WhenAlreadyInitialized_ShouldReturn409()
    {
        // Arrange - 先初始化
        var initRequest = new InitializeVaultRequest { MasterPassword = "Password123!" };
        await _client.PostAsJsonAsync("/api/vault/initialize", initRequest);

        // Act - 再次尝试初始化
        var response = await _client.PostAsJsonAsync("/api/vault/initialize", initRequest);

        // Assert - InvalidOperationException maps to 409 Conflict
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Unlock_WithCorrectPassword_ShouldReturn200AndSession()
    {
        // Arrange
        var password = "CorrectPassword123!";
        await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = password });

        var unlockRequest = new UnlockVaultRequest { MasterPassword = password };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vault/unlock", unlockRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionId.Should().NotBeNullOrEmpty();
        result.Data.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Unlock_WithIncorrectPassword_ShouldReturn401()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = "CorrectPassword123!" });

        var unlockRequest = new UnlockVaultRequest { MasterPassword = "WrongPassword456!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vault/unlock", unlockRequest);

        // Assert - CryptographicException maps to 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unlock_WhenNotInitialized_ShouldReturn409()
    {
        // Arrange
        var unlockRequest = new UnlockVaultRequest { MasterPassword = "SomePassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/vault/unlock", unlockRequest);

        // Assert - InvalidOperationException maps to 409 Conflict
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Lock_WithValidSession_ShouldReturn200()
    {
        // Arrange
        var initResponse = await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = "Password123!" });

        var initResult = await initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        var sessionId = initResult!.Data!.SessionId;

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vault/lock");
        request.Headers.Add("X-Vault-Session", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Lock_WithoutSessionHeader_ShouldReturn401()
    {
        // Act
        var response = await _client.PostAsync("/api/vault/lock", null);

        // Assert - VaultSessionMiddleware returns 401 for missing session
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithCorrectOldPassword_ShouldReturn200()
    {
        // Arrange - Initialize and get session
        var password = "OldPassword123!";
        var initResponse = await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = password });
        var initResult = await initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        var sessionId = initResult!.Data!.SessionId;

        var changeRequest = new ChangeMasterPasswordRequest
        {
            OldMasterPassword = password,
            NewMasterPassword = "NewPassword456!"
        };

        // Act - Include session header
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vault/change-password")
        {
            Content = JsonContent.Create(changeRequest)
        };
        request.Headers.Add("X-Vault-Session", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_ShouldInvalidateOldPassword()
    {
        // Arrange - Initialize and get session
        var password = "OldPassword123!";
        var initResponse = await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = password });
        var initResult = await initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        var sessionId = initResult!.Data!.SessionId;

        var changeRequest = new ChangeMasterPasswordRequest
        {
            OldMasterPassword = password,
            NewMasterPassword = "NewPassword456!"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vault/change-password")
        {
            Content = JsonContent.Create(changeRequest)
        };
        request.Headers.Add("X-Vault-Session", sessionId);
        await _client.SendAsync(request);

        // Act - 尝试用旧密码解锁
        var unlockResponse = await _client.PostAsJsonAsync("/api/vault/unlock",
            new UnlockVaultRequest { MasterPassword = password });

        // Assert - CryptographicException maps to 401
        unlockResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_ShouldAllowNewPassword()
    {
        // Arrange - Initialize and get session
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword456!";
        var initResponse = await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = oldPassword });
        var initResult = await initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        var sessionId = initResult!.Data!.SessionId;

        var changeRequest = new ChangeMasterPasswordRequest
        {
            OldMasterPassword = oldPassword,
            NewMasterPassword = newPassword
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vault/change-password")
        {
            Content = JsonContent.Create(changeRequest)
        };
        request.Headers.Add("X-Vault-Session", sessionId);
        await _client.SendAsync(request);

        // Act - 用新密码解锁
        var unlockResponse = await _client.PostAsJsonAsync("/api/vault/unlock",
            new UnlockVaultRequest { MasterPassword = newPassword });

        // Assert
        unlockResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await unlockResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectOldPassword_ShouldReturn401()
    {
        // Arrange - Initialize and get session
        var correctPassword = "CorrectPassword123!";
        var initResponse = await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = correctPassword });
        var initResult = await initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        var sessionId = initResult!.Data!.SessionId;

        var changeRequest = new ChangeMasterPasswordRequest
        {
            OldMasterPassword = "WrongOldPassword!",
            NewMasterPassword = "NewPassword456!"
        };

        // Act - Include session header
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/vault/change-password")
        {
            Content = JsonContent.Create(changeRequest)
        };
        request.Headers.Add("X-Vault-Session", sessionId);
        var response = await _client.SendAsync(request);

        // Assert - CryptographicException maps to 401
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteWorkflow_InitializeUnlockLock_ShouldWork()
    {
        // 1. 获取状态 - 未初始化
        var statusResponse1 = await _client.GetAsync("/api/vault/status");
        var status1 = await statusResponse1.Content.ReadFromJsonAsync<ApiResponse<VaultStatusResponse>>();
        status1!.Data!.IsInitialized.Should().BeFalse();

        // 2. 初始化
        var initResponse = await _client.PostAsJsonAsync("/api/vault/initialize",
            new InitializeVaultRequest { MasterPassword = "TestPassword123!" });
        initResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var initResult = await initResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        var sessionId1 = initResult!.Data!.SessionId;
        sessionId1.Should().NotBeNullOrEmpty();

        // 3. 获取状态 - 已初始化
        var statusResponse2 = await _client.GetAsync("/api/vault/status");
        var status2 = await statusResponse2.Content.ReadFromJsonAsync<ApiResponse<VaultStatusResponse>>();
        status2!.Data!.IsInitialized.Should().BeTrue();

        // 4. 锁定
        var lockRequest = new HttpRequestMessage(HttpMethod.Post, "/api/vault/lock");
        lockRequest.Headers.Add("X-Vault-Session", sessionId1);
        var lockResponse = await _client.SendAsync(lockRequest);
        lockResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. 解锁
        var unlockResponse = await _client.PostAsJsonAsync("/api/vault/unlock",
            new UnlockVaultRequest { MasterPassword = "TestPassword123!" });
        unlockResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var unlockResult = await unlockResponse.Content.ReadFromJsonAsync<ApiResponse<VaultSessionResponse>>();
        unlockResult!.Data!.SessionId.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        // 清理测试后的静态状态
        VaultService.ResetFailedAttemptsForTesting();

        _client.Dispose();
        _factory.Dispose();
    }
}
