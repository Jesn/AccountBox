using FluentAssertions;
using AccountBox.Api.Services;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;
using AccountBox.Data.DbContext;
using AccountBox.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Core.Tests;

/// <summary>
/// WebsiteService 级联删除测试
/// 测试网站删除时的安全保护逻辑（活跃账号检查、回收站账号确认）
/// </summary>
public class WebsiteServiceCascadeDeleteTests : IDisposable
{
    private readonly AccountBoxDbContext _context;
    private readonly WebsiteRepository _websiteRepo;
    private readonly WebsiteService _service;

    public WebsiteServiceCascadeDeleteTests()
    {
        var options = new DbContextOptionsBuilder<AccountBoxDbContext>()
            .UseInMemoryDatabase("TestDb_CascadeDelete_" + Guid.NewGuid())
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
    public async Task DeleteAsync_WithActiveAccounts_ShouldThrowActiveAccountsExistException()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // 添加活跃账号
        _context.Accounts.Add(new Account
        {
            WebsiteId = website.Id,
            Username = "activeuser",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16],
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _service.DeleteAsync(website.Id, confirmed: false);

        await act.Should().ThrowAsync<ActiveAccountsExistException>()
            .WithMessage($"*{website.Id}*")
            .Where(ex => ex.WebsiteId == website.Id && ex.ActiveAccountCount == 1);
    }

    [Fact]
    public async Task DeleteAsync_WithDeletedAccountsOnly_NotConfirmed_ShouldThrowConfirmationRequiredException()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // 添加已删除账号（在回收站）
        _context.Accounts.Add(new Account
        {
            WebsiteId = website.Id,
            Username = "deleteduser",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16],
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        var act = async () => await _service.DeleteAsync(website.Id, confirmed: false);

        await act.Should().ThrowAsync<ConfirmationRequiredException>()
            .WithMessage($"*{website.Id}*")
            .Where(ex => ex.WebsiteId == website.Id && ex.DeletedAccountCount == 1);
    }

    [Fact]
    public async Task DeleteAsync_WithDeletedAccountsOnly_Confirmed_ShouldSucceed()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // 添加已删除账号（在回收站）
        var deletedAccount = new Account
        {
            WebsiteId = website.Id,
            Username = "deleteduser",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16],
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(deletedAccount);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(website.Id, confirmed: true);

        // Assert
        var deletedWebsite = await _context.Websites.FindAsync(website.Id);
        deletedWebsite.Should().BeNull("网站应该被删除");

        // 验证级联删除：账号也应该被删除（数据库级联）
        var deletedAccountInDb = await _context.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == deletedAccount.Id);
        deletedAccountInDb.Should().BeNull("账号应该被级联删除");
    }

    [Fact]
    public async Task DeleteAsync_WithNoAccounts_ShouldSucceed()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(website.Id, confirmed: false);

        // Assert
        var deletedWebsite = await _context.Websites.FindAsync(website.Id);
        deletedWebsite.Should().BeNull("网站应该被删除");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentWebsite_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act & Assert
        var act = async () => await _service.DeleteAsync(nonExistentId, confirmed: false);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{nonExistentId}*");
    }

    [Fact]
    public async Task DeleteAsync_WithMixedAccounts_ShouldThrowActiveAccountsExistException()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // 添加活跃账号和已删除账号
        _context.Accounts.AddRange(
            new Account
            {
                WebsiteId = website.Id,
                Username = "activeuser",
                PasswordEncrypted = new byte[10],
                PasswordIV = new byte[12],
                PasswordTag = new byte[16],
                IsDeleted = false
            },
            new Account
            {
                WebsiteId = website.Id,
                Username = "deleteduser",
                PasswordEncrypted = new byte[10],
                PasswordIV = new byte[12],
                PasswordTag = new byte[16],
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act & Assert
        // 有活跃账号时，应该先抛出 ActiveAccountsExistException，而不是 ConfirmationRequiredException
        var act = async () => await _service.DeleteAsync(website.Id, confirmed: false);

        await act.Should().ThrowAsync<ActiveAccountsExistException>()
            .WithMessage($"*{website.Id}*")
            .Where(ex => ex.WebsiteId == website.Id && ex.ActiveAccountCount == 1);
    }

    [Fact]
    public async Task DeleteAsync_WithMultipleDeletedAccounts_Confirmed_ShouldDeleteAllCascade()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // 添加多个已删除账号
        var account1 = new Account
        {
            WebsiteId = website.Id,
            Username = "deleted1",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16],
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        var account2 = new Account
        {
            WebsiteId = website.Id,
            Username = "deleted2",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16],
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        _context.Accounts.AddRange(account1, account2);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(website.Id, confirmed: true);

        // Assert
        var deletedWebsite = await _context.Websites.FindAsync(website.Id);
        deletedWebsite.Should().BeNull("网站应该被删除");

        // 验证所有账号都被级联删除
        var remainingAccounts = await _context.Accounts
            .IgnoreQueryFilters()
            .Where(a => a.WebsiteId == website.Id)
            .CountAsync();
        remainingAccounts.Should().Be(0, "所有关联账号应该被级联删除");
    }

    [Fact]
    public async Task DeleteAsync_WithActiveAccounts_Confirmed_ShouldStillThrow()
    {
        // Arrange
        var website = new Website
        {
            Domain = "example.com",
            DisplayName = "Example Site"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        // 添加活跃账号
        _context.Accounts.Add(new Account
        {
            WebsiteId = website.Id,
            Username = "activeuser",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16],
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        // 即使 confirmed=true，有活跃账号时也应该抛出异常
        var act = async () => await _service.DeleteAsync(website.Id, confirmed: true);

        await act.Should().ThrowAsync<ActiveAccountsExistException>()
            .WithMessage($"*{website.Id}*");
    }
}
