using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.JWT;
using App.Application.UseCases.Adverts.AttachAdvertImages;
using App.Application.UseCases.Adverts.ImportAdverts;
using App.Application.UseCases.Users.RefreshToken;
using App.Application.UseCases.Users.SignIn;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Errors;
using App.Domain.Repositories;
using App.Domain.Shared;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Security;

public class SecurityAuditAndRbacTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserSessionRepository> _sessionRepository = new();
    private readonly Mock<IJwtTokenGenerator> _jwtGenerator = new();
    private readonly Mock<IRefreshTokenGenerator> _tokenGenerator = new();
    private readonly Mock<ISecurityAuditLogger> _auditLogger = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAdvertRepository> _advertRepository = new();
    private readonly Mock<IHtmlSanitizerService> _htmlSanitizer = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IImageProcessor> _imageProcessor = new();

    [Fact]
    public async Task SignIn_NonExistentUser_LogsLoginFailed()
    {
        // Arrange
        _userRepository.Setup(x => x.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new SignInCommandHandler(
            _userRepository.Object,
            _sessionRepository.Object,
            _jwtGenerator.Object,
            _tokenGenerator.Object,
            _auditLogger.Object,
            _unitOfWork.Object);

        // Act
        var result = await handler.Handle(new SignInCommand("unknown@example.com", "Password123!"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);

        _auditLogger.Verify(x => x.LogAsync(
            SecurityEventType.LoginFailed,
            false,
            null,
            "unknown@example.com",
            It.Is<string>(d => d.Contains("does not exist")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignIn_InvalidPassword_LogsLoginFailed()
    {
        // Arrange
        var user = User.Create("user@example.com", BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!"));
        _userRepository.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new SignInCommandHandler(
            _userRepository.Object,
            _sessionRepository.Object,
            _jwtGenerator.Object,
            _tokenGenerator.Object,
            _auditLogger.Object,
            _unitOfWork.Object);

        // Act
        var result = await handler.Handle(new SignInCommand("user@example.com", "WrongPassword!"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);

        _auditLogger.Verify(x => x.LogAsync(
            SecurityEventType.LoginFailed,
            false,
            user.Guid,
            "user@example.com",
            It.Is<string>(d => d.Contains("Invalid password")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignIn_SuccessfulLogin_LogsLoginSucceeded()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var user = User.Create("user@example.com", BCrypt.Net.BCrypt.HashPassword(password));
        user.VerifyEmail();

        _userRepository.Setup(x => x.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtGenerator.Setup(x => x.GenerateToken(user))
            .Returns(("access-token", "jwt-id"));

        _tokenGenerator.Setup(x => x.GenerateToken()).Returns("refresh-token");
        _tokenGenerator.Setup(x => x.Hash("refresh-token")).Returns("refresh-token-hash");

        var handler = new SignInCommandHandler(
            _userRepository.Object,
            _sessionRepository.Object,
            _jwtGenerator.Object,
            _tokenGenerator.Object,
            _auditLogger.Object,
            _unitOfWork.Object);

        // Act
        var result = await handler.Handle(new SignInCommand("user@example.com", password), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);

        _auditLogger.Verify(x => x.LogAsync(
            SecurityEventType.LoginSucceeded,
            true,
            user.Guid,
            "user@example.com",
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_InvalidSession_LogsRefreshFailed()
    {
        // Arrange
        _tokenGenerator.Setup(x => x.Hash("invalid-token")).Returns("hash");
        _sessionRepository.Setup(x => x.GetByRefreshTokenHashAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        var handler = new RefreshTokenCommandHandler(
            _sessionRepository.Object,
            _userRepository.Object,
            _jwtGenerator.Object,
            _tokenGenerator.Object,
            _auditLogger.Object,
            _unitOfWork.Object);

        // Act
        var result = await handler.Handle(new RefreshTokenCommand("invalid-token"), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);

        _auditLogger.Verify(x => x.LogAsync(
            SecurityEventType.RefreshFailed,
            false,
            null,
            null,
            It.Is<string>(d => d.Contains("invalid or expired")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ImportAdverts_NonAdminUser_ReturnsForbidden()
    {
        // Arrange
        var normalUser = User.Create("normal@example.com", "hash");
        _userRepository.Setup(x => x.GetByUuidAsync(normalUser.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(normalUser);

        var handler = new ImportAdvertsCommandHandler(
            _advertRepository.Object,
            _userRepository.Object,
            _unitOfWork.Object,
            _htmlSanitizer.Object);

        var command = new ImportAdvertsCommand([], normalUser.Guid);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AdminErrors.Forbidden.Code, result.Error.Code);
    }

    [Fact]
    public async Task AttachAdvertImages_NonAdminUser_ReturnsForbidden()
    {
        // Arrange
        var normalUser = User.Create("normal@example.com", "hash");
        _userRepository.Setup(x => x.GetByUuidAsync(normalUser.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(normalUser);

        var handler = new AttachAdvertImagesCommandHandler(
            _advertRepository.Object,
            _userRepository.Object,
            _unitOfWork.Object,
            _fileStorage.Object,
            _imageProcessor.Object);

        var command = new AttachAdvertImagesCommand([], normalUser.Guid);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(AdminErrors.Forbidden.Code, result.Error.Code);
    }
}

