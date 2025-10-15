using Moq;
using FluentAssertions;
using System.Text;
using AccountBox.Api.Services;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Account;
using AccountBox.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Core.Tests;

/// <summary>
/// AccountService 集成测试
/// 使用 InMemory 数据库和 Mock 加密服务测试账号管理业务逻辑
/// </summary>
public class AccountServiceTests : IDisposable
{
    private readonly AccountBoxDbContext _context;
    private readonly AccountRepository _accountRepo;
    private readonly WebsiteRepository _websiteRepo;
    private readonly Mock<IEncryptionService> _mockEncryption;
    private readonly AccountService _service;
    private readonly byte[] _testVaultKey;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<AccountBoxDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;

        _context = new AccountBoxDbContext(options);
        _accountRepo = new AccountRepository(_context);
        _websiteRepo = new WebsiteRepository(_context);
        _mockEncryption = new Mock<IEncryptionService>();
        _service = new AccountService(_accountRepo, _websiteRepo, _mockEncryption.Object);
        _testVaultKey = new byte[32]; // 256-bit key
        Array.Fill(_testVaultKey, (byte)0x42);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnDecryptedAccounts()
    {
        // Arrange
        var website = new Website { Domain = "example.com", DisplayName = "Example" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var encryptedPassword = Encoding.UTF8.GetBytes("encrypted");
        var iv = new byte[12];
        var tag = new byte[16];

        _context.Accounts.Add(new Account
        {
            WebsiteId = website.Id,
            Username = "user1",
            PasswordEncrypted = encryptedPassword,
            PasswordIV = iv,
            PasswordTag = tag
        });
        await _context.SaveChangesAsync();

        _mockEncryption.Setup(e => e.Decrypt(encryptedPassword, iv, tag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("mypassword"));

        // Act
        var result = await _service.GetPagedAsync(1, 10, null, _testVaultKey);

        // Assert
        result.Should().NotBeNull();
        var itemsList = result.Items.ToList();
        itemsList.Should().HaveCount(1);
        itemsList[0].Username.Should().Be("user1");
        itemsList[0].Password.Should().Be("mypassword");
        itemsList[0].WebsiteDomain.Should().Be("example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDecryptedAccount()
    {
        // Arrange
        var website = new Website { Domain = "test.com", DisplayName = "Test" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var encryptedPassword = Encoding.UTF8.GetBytes("encrypted");
        var encryptedNotes = Encoding.UTF8.GetBytes("encrypted_notes");
        var iv = new byte[12];
        var tag = new byte[16];

        var account = new Account
        {
            WebsiteId = website.Id,
            Username = "testuser",
            PasswordEncrypted = encryptedPassword,
            PasswordIV = iv,
            PasswordTag = tag,
            NotesEncrypted = encryptedNotes,
            NotesIV = iv,
            NotesTag = tag,
            Tags = "important"
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        _mockEncryption.Setup(e => e.Decrypt(encryptedPassword, iv, tag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("secretpass"));
        _mockEncryption.Setup(e => e.Decrypt(encryptedNotes, iv, tag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("my notes"));

        // Act
        var result = await _service.GetByIdAsync(account.Id, _testVaultKey);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.Password.Should().Be("secretpass");
        result.Notes.Should().Be("my notes");
        result.Tags.Should().Be("important");
    }

    [Fact]
    public async Task CreateAsync_ShouldEncryptPasswordAndNotes()
    {
        // Arrange
        var website = new Website { Domain = "test.com", DisplayName = "Test" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var request = new CreateAccountRequest
        {
            WebsiteId = website.Id,
            Username = "newuser",
            Password = "plainpassword",
            Notes = "plain notes",
            Tags = "test"
        };

        var encryptedPassword = Encoding.UTF8.GetBytes("encrypted_pwd");
        var encryptedNotes = Encoding.UTF8.GetBytes("encrypted_notes");
        var iv = new byte[12];
        var tag = new byte[16];

        _mockEncryption.Setup(e => e.Encrypt(
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "plainpassword"),
            _testVaultKey))
            .Returns((encryptedPassword, iv, tag));
        _mockEncryption.Setup(e => e.Encrypt(
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "plain notes"),
            _testVaultKey))
            .Returns((encryptedNotes, iv, tag));

        _mockEncryption.Setup(e => e.Decrypt(encryptedPassword, iv, tag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("plainpassword"));
        _mockEncryption.Setup(e => e.Decrypt(encryptedNotes, iv, tag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("plain notes"));

        // Act
        var result = await _service.CreateAsync(request, _testVaultKey);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.Password.Should().Be("plainpassword");
        result.Notes.Should().Be("plain notes");

        var savedAccount = await _context.Accounts.FindAsync(result.Id);
        savedAccount.Should().NotBeNull();
        savedAccount!.PasswordEncrypted.Should().BeEquivalentTo(encryptedPassword);
        savedAccount.NotesEncrypted.Should().BeEquivalentTo(encryptedNotes);
    }

    [Fact]
    public async Task UpdateAsync_ShouldEncryptNewPassword()
    {
        // Arrange
        var website = new Website { Domain = "example.com", DisplayName = "Example" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var oldEncrypted = Encoding.UTF8.GetBytes("old_encrypted");
        var oldIv = new byte[12];
        var oldTag = new byte[16];

        var existing = new Account
        {
            WebsiteId = website.Id,
            Username = "olduser",
            PasswordEncrypted = oldEncrypted,
            PasswordIV = oldIv,
            PasswordTag = oldTag
        };
        _context.Accounts.Add(existing);
        await _context.SaveChangesAsync();

        var request = new UpdateAccountRequest
        {
            Username = "updateduser",
            Password = "newpassword",
            Notes = null,
            Tags = "updated"
        };

        var newEncrypted = Encoding.UTF8.GetBytes("new_encrypted");
        var newIv = new byte[12];
        var newTag = new byte[16];

        _mockEncryption.Setup(e => e.Encrypt(
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "newpassword"),
            _testVaultKey))
            .Returns((newEncrypted, newIv, newTag));
        _mockEncryption.Setup(e => e.Decrypt(newEncrypted, newIv, newTag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("newpassword"));

        // Act
        var result = await _service.UpdateAsync(existing.Id, request, _testVaultKey);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("updateduser");
        result.Password.Should().Be("newpassword");
        result.Tags.Should().Be("updated");
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkAsDeleted()
    {
        // Arrange
        var website = new Website { Domain = "test.com", DisplayName = "Test" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var account = new Account
        {
            WebsiteId = website.Id,
            Username = "user1",
            PasswordEncrypted = new byte[10],
            PasswordIV = new byte[12],
            PasswordTag = new byte[16]
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        // Act
        await _service.SoftDeleteAsync(account.Id);

        // Assert
        var deleted = await _context.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == account.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithoutNotes_ShouldOnlyEncryptPassword()
    {
        // Arrange
        var website = new Website { Domain = "test.com", DisplayName = "Test" };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var request = new CreateAccountRequest
        {
            WebsiteId = website.Id,
            Username = "user",
            Password = "pass",
            Notes = null
        };

        var encrypted = Encoding.UTF8.GetBytes("encrypted");
        var iv = new byte[12];
        var tag = new byte[16];

        _mockEncryption.Setup(e => e.Encrypt(It.IsAny<byte[]>(), _testVaultKey))
            .Returns((encrypted, iv, tag));
        _mockEncryption.Setup(e => e.Decrypt(encrypted, iv, tag, _testVaultKey))
            .Returns(Encoding.UTF8.GetBytes("pass"));

        // Act
        var result = await _service.CreateAsync(request, _testVaultKey);

        // Assert
        var savedAccount = await _context.Accounts.FindAsync(result.Id);
        savedAccount.Should().NotBeNull();
        savedAccount!.NotesEncrypted.Should().BeNull();
        savedAccount.NotesIV.Should().BeNull();
        savedAccount.NotesTag.Should().BeNull();
        result.Notes.Should().BeNull();
    }
}
