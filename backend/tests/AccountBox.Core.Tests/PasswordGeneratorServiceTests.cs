using FluentAssertions;
using AccountBox.Core.Services;
using AccountBox.Core.Models.PasswordGenerator;

namespace AccountBox.Core.Tests;

/// <summary>
/// PasswordGeneratorService 单元测试
/// 测试密码生成、强度计算、字符集配置等功能
/// </summary>
public class PasswordGeneratorServiceTests
{
    private readonly PasswordGeneratorService _service;

    public PasswordGeneratorServiceTests()
    {
        _service = new PasswordGeneratorService();
    }

    [Fact]
    public void Generate_WithValidRequest_ShouldReturnPassword()
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

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Should().NotBeNull();
        result.Password.Should().NotBeNullOrEmpty();
        result.Password.Length.Should().Be(16);
        result.Strength.Should().NotBeNull();
    }

    [Fact]
    public void Generate_WithOnlyUppercase_ShouldContainOnlyUppercase()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 20,
            IncludeUppercase = true,
            IncludeLowercase = false,
            IncludeNumbers = false,
            IncludeSymbols = false,
            ExcludeAmbiguous = false
        };

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Password.Should().NotBeNullOrEmpty();
        result.Password.Should().MatchRegex("^[A-Z]+$");
    }

    [Fact]
    public void Generate_WithOnlyLowercase_ShouldContainOnlyLowercase()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 20,
            IncludeUppercase = false,
            IncludeLowercase = true,
            IncludeNumbers = false,
            IncludeSymbols = false,
            ExcludeAmbiguous = false
        };

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Password.Should().NotBeNullOrEmpty();
        result.Password.Should().MatchRegex("^[a-z]+$");
    }

    [Fact]
    public void Generate_WithOnlyNumbers_ShouldContainOnlyNumbers()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 20,
            IncludeUppercase = false,
            IncludeLowercase = false,
            IncludeNumbers = true,
            IncludeSymbols = false,
            ExcludeAmbiguous = false
        };

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Password.Should().NotBeNullOrEmpty();
        result.Password.Should().MatchRegex("^[0-9]+$");
    }

    [Fact]
    public void Generate_WithExcludeAmbiguous_ShouldNotContainAmbiguousCharacters()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 50, // 较长的密码以增加检测概率
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = false,
            ExcludeAmbiguous = true
        };

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Password.Should().NotBeNullOrEmpty();
        result.Password.Should().NotContain("0");
        result.Password.Should().NotContain("O");
        result.Password.Should().NotContain("1");
        result.Password.Should().NotContain("l");
        result.Password.Should().NotContain("I");
    }

    [Fact]
    public void Generate_WithMinimumLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 8,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Password.Length.Should().Be(8);
    }

    [Fact]
    public void Generate_WithMaximumLength_ShouldReturnCorrectLength()
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

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Password.Length.Should().Be(128);
    }

    [Fact]
    public void Generate_WithLengthLessThan8_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 7,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        // Act & Assert
        var act = () => _service.Generate(request);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*8*");
    }

    [Fact]
    public void Generate_WithLengthGreaterThan128_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 129,
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        // Act & Assert
        var act = () => _service.Generate(request);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*128*");
    }

    [Fact]
    public void Generate_WithNoCharacterTypes_ShouldThrowArgumentException()
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

        // Act & Assert
        var act = () => _service.Generate(request);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*至少*");
    }

    [Fact]
    public void Generate_MultipleGenerations_ShouldReturnDifferentPasswords()
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

        // Act
        var result1 = _service.Generate(request);
        var result2 = _service.Generate(request);
        var result3 = _service.Generate(request);

        // Assert
        result1.Password.Should().NotBe(result2.Password);
        result2.Password.Should().NotBe(result3.Password);
        result1.Password.Should().NotBe(result3.Password);
    }

    [Fact]
    public void CalculateStrength_WithEmptyPassword_ShouldReturnWeakStrength()
    {
        // Arrange
        var password = "";

        // Act
        var result = _service.CalculateStrength(password);

        // Assert
        result.Should().NotBeNull();
        result.Strength.Should().NotBeNull();
        result.Strength.Score.Should().Be(0);
        result.Strength.Level.Should().Be("Weak");
        result.Strength.Length.Should().Be(0);
        result.Strength.Entropy.Should().Be(0);
    }

    [Fact]
    public void CalculateStrength_WithShortPassword_ShouldReturnWeakStrength()
    {
        // Arrange
        var password = "abc";

        // Act
        var result = _service.CalculateStrength(password);

        // Assert
        result.Strength.Level.Should().Be("Weak");
        result.Strength.Score.Should().BeLessThan(40);
        result.Strength.HasLowercase.Should().BeTrue();
        result.Strength.HasUppercase.Should().BeFalse();
        result.Strength.HasNumbers.Should().BeFalse();
        result.Strength.HasSymbols.Should().BeFalse();
    }

    [Fact]
    public void CalculateStrength_WithMediumPassword_ShouldReturnMediumStrength()
    {
        // Arrange
        var password = "Password123";

        // Act
        var result = _service.CalculateStrength(password);

        // Assert
        result.Strength.Level.Should().BeOneOf("Medium", "Strong");
        result.Strength.HasUppercase.Should().BeTrue();
        result.Strength.HasLowercase.Should().BeTrue();
        result.Strength.HasNumbers.Should().BeTrue();
    }

    [Fact]
    public void CalculateStrength_WithStrongPassword_ShouldReturnStrongOrVeryStrong()
    {
        // Arrange
        var password = "MyStr0ng!P@ssw0rd#2024";

        // Act
        var result = _service.CalculateStrength(password);

        // Assert
        result.Strength.Level.Should().BeOneOf("Strong", "VeryStrong");
        result.Strength.Score.Should().BeGreaterThan(60);
        result.Strength.HasUppercase.Should().BeTrue();
        result.Strength.HasLowercase.Should().BeTrue();
        result.Strength.HasNumbers.Should().BeTrue();
        result.Strength.HasSymbols.Should().BeTrue();
        result.Strength.Entropy.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateStrength_WithOnlyNumbers_ShouldDetectCorrectly()
    {
        // Arrange
        var password = "12345678";

        // Act
        var result = _service.CalculateStrength(password);

        // Assert
        result.Strength.HasNumbers.Should().BeTrue();
        result.Strength.HasUppercase.Should().BeFalse();
        result.Strength.HasLowercase.Should().BeFalse();
        result.Strength.HasSymbols.Should().BeFalse();
    }

    [Fact]
    public void CalculateStrength_WithSymbols_ShouldDetectSymbols()
    {
        // Arrange
        var password = "Pass!@#$";

        // Act
        var result = _service.CalculateStrength(password);

        // Assert
        result.Strength.HasSymbols.Should().BeTrue();
    }

    [Fact]
    public void Generate_WithAllCharacterTypes_ShouldIncludeAllTypes()
    {
        // Arrange
        var request = new GeneratePasswordRequest
        {
            Length = 50, // 较长的密码以增加包含所有类型的概率
            IncludeUppercase = true,
            IncludeLowercase = true,
            IncludeNumbers = true,
            IncludeSymbols = true,
            ExcludeAmbiguous = false
        };

        // Act
        var result = _service.Generate(request);

        // Assert
        result.Strength.HasUppercase.Should().BeTrue();
        result.Strength.HasLowercase.Should().BeTrue();
        result.Strength.HasNumbers.Should().BeTrue();
        result.Strength.HasSymbols.Should().BeTrue();
    }
}
