using App.Application.Abstractions.Auth;
using App.Application.UseCases.Users.Login;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;
using Moq;
using Xunit;

namespace App.Application.Tests.UseCases.Users.Login;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailDoesNotExist_ReturnsUnauthorized()
    {
        // 1. Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var handler = new LoginCommandHandler(userRepositoryMock.Object, jwtTokenGeneratorMock.Object);

        var command = new LoginCommand("nobody@example.com", "whatever123");

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        jwtTokenGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ReturnsUnauthorizedWithSameErrorAsMissingEmail()
    {
        // 1. Arrange
        var correctHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var existingUser = new User("user@example.com", correctHash);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        var handler = new LoginCommandHandler(userRepositoryMock.Object, jwtTokenGeneratorMock.Object);

        var command = new LoginCommand("user@example.com", "wrong-password");

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        jwtTokenGeneratorMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsGeneratedToken()
    {
        // 1. Arrange
        var correctHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var existingUser = new User("user@example.com", correctHash);

        var userRepositoryMock = new Mock<IUserRepository>();
        userRepositoryMock
            .Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        jwtTokenGeneratorMock
            .Setup(j => j.GenerateToken(existingUser))
            .Returns("fake-jwt-token");

        var handler = new LoginCommandHandler(userRepositoryMock.Object, jwtTokenGeneratorMock.Object);
        var command = new LoginCommand("user@example.com", "correct-password");

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("fake-jwt-token", result.Value);
        jwtTokenGeneratorMock.Verify(j => j.GenerateToken(existingUser), Times.Once);
    }
}