using App.Application.Abstractions.Interfaces;
using App.Application.UseCases.Users.ChangePassword;
using App.Application.UseCases.Users.ResetPassword;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Infrastructure.Auth;
using Moq;

namespace App.Application.Tests.UseCases.Users.Password;

public class PasswordSessionRevocationTests
{
    private const string OldPassword = "OldPassword1!";
    private const string NewPassword = "NewPassword1!";
    private const string ResetToken = "reset-token";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserSessionRepository> _sessionRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshTokenGenerator _tokens = new();

    private readonly User _user = User.Create("user@example.com", BCrypt.Net.BCrypt.HashPassword(OldPassword));
    private readonly UserSession _current = UserSession.Create(1, "hash-1", "current-jwt", DateTime.UtcNow.AddDays(30));
    private readonly UserSession _other = UserSession.Create(1, "hash-2", "other-jwt", DateTime.UtcNow.AddDays(30));

    public PasswordSessionRevocationTests()
    {
        _sessionRepository
            .Setup(x => x.GetActiveByUserIdAsync(_user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_current, _other]);
        _users
            .Setup(x => x.GetByUuidAsync(_user.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
        _users
            .Setup(x => x.GetByPasswordResetTokenHashAsync(_tokens.Hash(ResetToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);
    }

    [Fact]
    public async Task ResetPassword_RevokesAllSessions()
    {
        _user.SetPasswordResetToken(_tokens.Hash(ResetToken), DateTime.UtcNow.AddMinutes(30));

        var result = await Reset(ResetToken);

        Assert.True(result.IsSuccess);
        Assert.True(_current.Redeemed);
        Assert.True(_other.Redeemed);
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_KeepsSessions()
    {
        _user.SetPasswordResetToken(_tokens.Hash(ResetToken), DateTime.UtcNow.AddMinutes(-1));

        var result = await Reset(ResetToken);

        Assert.True(result.IsFailure);
        Assert.False(_current.Redeemed);
        Assert.False(_other.Redeemed);
    }

    [Fact]
    public async Task ChangePassword_RevokesOtherSessionsAndKeepsCurrent()
    {
        var result = await Change(OldPassword, "current-jwt");

        Assert.True(result.IsSuccess);
        Assert.False(_current.Redeemed);
        Assert.True(_other.Redeemed);
    }

    [Fact]
    public async Task ChangePassword_WithoutKnownSession_RevokesAllSessions()
    {
        var result = await Change(OldPassword, null);

        Assert.True(result.IsSuccess);
        Assert.True(_current.Redeemed);
        Assert.True(_other.Redeemed);
    }

    [Fact]
    public async Task ChangePassword_WithWrongPassword_KeepsSessions()
    {
        var result = await Change("wrong", "current-jwt");

        Assert.True(result.IsFailure);
        Assert.False(_other.Redeemed);
    }

    private Task<App.Domain.Shared.Result> Reset(string token) =>
        new ResetPasswordCommandHandler(
                _users.Object, _sessionRepository.Object, _tokens,
                new Mock<ISecurityAuditLogger>().Object, _unitOfWork.Object)
            .Handle(new ResetPasswordCommand(token, NewPassword), CancellationToken.None);

    private Task<App.Domain.Shared.Result> Change(string currentPassword, string? jwtId) =>
        new ChangePasswordCommandHandler(_users.Object, _sessionRepository.Object, _unitOfWork.Object)
            .Handle(new ChangePasswordCommand(_user.Guid, currentPassword, NewPassword, jwtId), CancellationToken.None);
}
