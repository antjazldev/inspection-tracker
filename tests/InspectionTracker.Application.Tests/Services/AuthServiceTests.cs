using FluentAssertions;
using InspectionTracker.Application.Dtos;
using InspectionTracker.Application.Exceptions;
using InspectionTracker.Application.Interfaces;
using InspectionTracker.Application.Services;
using InspectionTracker.Domain.Entities;
using Moq;

namespace InspectionTracker.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepoMock.Object);
    }

    private static RegisterDto ValidRegister() => new(
        Email: "luis@example.com",
        DisplayName: "Luis",
        Password: "Sup3rSecret!");

    [Fact]
    public async Task RegisterAsync_WithValidData_SavesUserWithHashedPassword()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.RegisterAsync(ValidRegister());

        result.Email.Should().Be("luis@example.com");
        _userRepoMock.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.PasswordHash != "Sup3rSecret!" &&
                !string.IsNullOrWhiteSpace(u.PasswordHash)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailTaken_ThrowsValidation()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("luis@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "luis@example.com" });

        Func<Task> act = () => _sut.RegisterAsync(ValidRegister());

        await act.Should().ThrowAsync<BusinessValidationException>()
            .WithMessage("*already*");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task RegisterAsync_WithWeakPassword_ThrowsValidation(string password)
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var dto = ValidRegister() with { Password = password };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<BusinessValidationException>()
            .WithMessage("*password*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task RegisterAsync_WithInvalidEmail_ThrowsValidation(string email)
    {
        var dto = ValidRegister() with { Email = email };

        Func<Task> act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<BusinessValidationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "luis@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sup3rSecret!")
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("luis@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginDto("luis@example.com", "Sup3rSecret!"));

        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Email = "luis@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sup3rSecret!")
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("luis@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Func<Task> act = () => _sut.LoginAsync(new LoginDto("luis@example.com", "wrong-password"));

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsSameUnauthorizedMessage()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> act = () => _sut.LoginAsync(new LoginDto("ghost@example.com", "whatever"));

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid credentials*");
    }
}
