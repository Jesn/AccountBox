using System.Net;
using System.Net.Http.Json;
using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.PasswordGenerator;
using AccountBox.Core.Models.Vault;
using AccountBox.Data.DbContext;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AccountBox.Api.Tests;

/// <summary>
/// PasswordGeneratorController HTTP API 集成测试
/// 测试完整的密码生成和强度计算 API 流程
/// </summary>
[Collection("VaultService Tests")]
public class PasswordGeneratorControllerTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName;
    private readonly string _sessionId;

    public PasswordGeneratorControllerTests()
    {
        // 清除静态状态
        VaultService.ResetFailedAttemptsForTesting();

        // 为每个测试创建唯一的数据库名称
        _databaseName = $"TestDb_PasswordGenerator_{Guid.NewGuid()}";

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
    public async Task Generate_WithValidRequest_ShouldReturn200AndPassword()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 16,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/generate")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratePasswordResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Password.Should().NotBeNullOrEmpty();
        result.Data.Password.Length.Should().Be(16);
        result.Data.Strength.Should().NotBeNull();
        result.Data.Strength.Score.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Generate_WithMinimumLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 8,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = false,
            ExcludeAmbiguous = false
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/generate")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratePasswordResponse>>();
        result!.Data!.Password.Length.Should().Be(8);
    }

    [Fact]
    public async Task Generate_WithMaximumLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 128,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/generate")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratePasswordResponse>>();
        result!.Data!.Password.Length.Should().Be(128);
    }

    [Fact]
    public async Task Generate_WithInvalidLength_ShouldReturn400()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 5, // 小于最小值 8
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/generate")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratePasswordResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be("INVALID_REQUEST");
    }

    [Fact]
    public async Task Generate_WithNoCharacterTypes_ShouldReturn400()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 16,
            IncludeUppercase = false,
            IncludeLowercase = false,
            IncludeNumbers = false,
            IncludeSymbols = false,
            ExcludeAmbiguous = false
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/generate")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratePasswordResponse>>();
        result!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Generate_WithExcludeAmbiguous_ShouldNotContainAmbiguousChars()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 50,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = false,
            ExcludeAmbiguous = true
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/generate")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<GeneratePasswordResponse>>();
        var password = result!.Data!.Password;

        password.Should().NotContain("0");
        password.Should().NotContain("O");
        password.Should().NotContain("1");
        password.Should().NotContain("l");
        password.Should().NotContain("I");
    }

    [Fact]
    public async Task CalculateStrength_WithStrongPassword_ShouldReturnHighScore()
    {
        // Arrange
        var request = new CalculateStrengthRequest
        {
            Password = "MyStr0ng!P@ssw0rd#2024"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/strength")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PasswordStrengthResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Strength.Should().NotBeNull();
        result.Data.Strength.Score.Should().BeGreaterThan(60);
        result.Data.Strength.Level.Should().BeOneOf("Strong", "VeryStrong");
        result.Data.Strength.HasUppercase.Should().BeTrue();
        result.Data.Strength.HasLowercase.Should().BeTrue();
        result.Data.Strength.HasNumbers.Should().BeTrue();
        result.Data.Strength.HasSymbols.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateStrength_WithWeakPassword_ShouldReturnLowScore()
    {
        // Arrange
        var request = new CalculateStrengthRequest
        {
            Password = "abc"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/strength")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PasswordStrengthResponse>>();
        result!.Data!.Strength.Level.Should().Be("Weak");
        result.Data.Strength.Score.Should().BeLessThan(40);
    }

    [Fact]
    public async Task CalculateStrength_WithEmptyPassword_ShouldReturnZeroScore()
    {
        // Arrange
        var request = new CalculateStrengthRequest
        {
            Password = ""
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/password-generator/strength")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Vault-Session", _sessionId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PasswordStrengthResponse>>();
        result!.Data!.Strength.Score.Should().Be(0);
        result.Data.Strength.Level.Should().Be("Weak");
        result.Data.Strength.Length.Should().Be(0);
    }

    [Fact]
    public async Task Generate_WithoutSession_ShouldReturn401()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 16,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        // Act - 不添加 X-Vault-Session 头
        var response = await _client.PostAsJsonAsync("/api/password-generator/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CalculateStrength_WithoutSession_ShouldReturn401()
    {
        // Arrange
        var request = new CalculateStrengthRequest
        {
            Password = "test123"
        };

        // Act - 不添加 X-Vault-Session 头
        var response = await _client.PostAsJsonAsync("/api/password-generator/strength", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public void Dispose()
    {
        // 清理测试后的静态状态
        VaultService.ResetFailedAttemptsForTesting();

        _client.Dispose();
        _factory.Dispose();
    }
}
