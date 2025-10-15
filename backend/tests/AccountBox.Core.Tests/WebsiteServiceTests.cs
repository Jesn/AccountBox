using FluentAssertions;
using AccountBox.Api.Services;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;
using AccountBox.Core.Models.Website;
using AccountBox.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Core.Tests;

/// <summary>
/// WebsiteService 集成测试
/// 使用 InMemory 数据库测试网站管理业务逻辑
/// </summary>
public class WebsiteServiceTests : IDisposable
{
    private readonly AccountBoxDbContext _context;
    private readonly WebsiteRepository _websiteRepo;
    private readonly WebsiteService _service;

    public WebsiteServiceTests()
    {
        var options = new DbContextOptionsBuilder<AccountBoxDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;

        _context = new AccountBoxDbContext(options);
        _websiteRepo = new WebsiteRepository(_context);
        _service = new WebsiteService(_websiteRepo);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResult()
    {
        // Arrange
        _context.Websites.AddRange(
            new Website { Domain = "example.com", DisplayName = "Example" },
            new Website { Domain = "test.com", DisplayName = "Test" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPagedAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        var itemsList = result.Items.ToList();
        itemsList.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnWebsite()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example",
            Tags = "test"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(website.Id);

        // Assert
        result.Should().NotBeNull();
        result.Domain.Should().Be("example.com");
        result.DisplayName.Should().Be("Example");
        result.Tags.Should().Be("test");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedWebsite()
    {
        // Arrange
        var request = new CreateWebsiteRequest
        {
            Domain = "newsite.com",
            DisplayName = "New Site",
            Tags = "new,test"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Domain.Should().Be("newsite.com");
        result.DisplayName.Should().Be("New Site");
        result.Tags.Should().Be("new,test");
        result.ActiveAccountCount.Should().Be(0);
        result.DeletedAccountCount.Should().Be(0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedWebsite()
    {
        // Arrange
        var website = new Website
        {
            Domain = "old.com",
            DisplayName = "Old"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var request = new UpdateWebsiteRequest
        {
            Domain = "updated.com",
            DisplayName = "Updated",
            Tags = "updated"
        };

        // Act
        var result = await _service.UpdateAsync(website.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(website.Id);
        result.Domain.Should().Be("updated.com");
        result.DisplayName.Should().Be("Updated");
        result.Tags.Should().Be("updated");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteWebsite()
    {
        // Arrange
        var website = new Website
        {
            Domain = "delete.com",
            DisplayName = "Delete"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(website.Id);

        // Assert
        var deleted = await _context.Websites.FindAsync(website.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountCountAsync_ShouldReturnCounts()
    {
        // Arrange
        var website = new Website { Domain = "test.com", DisplayName = "Test" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        _context.Accounts.AddRange(
            new Account
            {
                WebsiteId = website.Id,
                Username = "user1",
                PasswordEncrypted = new byte[10],
                PasswordIV = new byte[12],
                PasswordTag = new byte[16],
                IsDeleted = false
            },
            new Account
            {
                WebsiteId = website.Id,
                Username = "user2",
                PasswordEncrypted = new byte[10],
                PasswordIV = new byte[12],
                PasswordTag = new byte[16],
                IsDeleted = true
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAccountCountAsync(website.Id);

        // Assert
        result.Should().NotBeNull();
        result.ActiveCount.Should().Be(1);
        result.DeletedCount.Should().Be(1);
        result.TotalCount.Should().Be(2);
    }
}
