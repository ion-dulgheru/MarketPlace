using App.Application.Abstractions.JWT;
using App.Application.UseCases.Users.Logout;
using App.Application.UseCases.Users.RefreshToken;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Infrastructure.Auth;
using Moq;

namespace App.Application.Tests.UseCases.Users.Logout;

public class LogoutCommandHandlerTests
{
    private const string RefreshToken = "refresh-token";

    private readonly Mock<IUserSessionRepository> _sessions = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshTokenGenerator _tokens = new();

    private readonly UserSession _session;

    public LogoutCommandHandlerTests()
    {
        _session = UserSession.Create(1, _tokens.Hash(RefreshToken), "jwt-id", DateTime.UtcNow.AddDays(30));

        _sessions
            .Setup(x => x.GetByRefreshTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string hash, CancellationToken _) => _session.RefreshTokenHash == hash ? _session : null);
    }

    [Fact]
    public async Task Logout_RevokesSession()
    {
        var result = await Logout(RefreshToken);

        Assert.True(result.IsSuccess);
        Assert.True(_session.Redeemed);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_WithUnknownToken_SucceedsWithoutSaving()
    {
        var result = await Logout("unknown-token");

        Assert.True(result.IsSuccess);
        Assert.False(_session.Redeemed);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Refresh_AfterLogout_Fails()
    {
        await Logout(RefreshToken);

        var result = await new RefreshTokenCommandHandler(
                _sessions.Object, _users.Object, _jwt.Object, _tokens, _unitOfWork.Object)
            .Handle(new RefreshTokenCommand(RefreshToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);
    }

    private Task<Domain.Shared.Result> Logout(string refreshToken) =>
        new LogoutCommandHandler(_sessions.Object, _tokens, _unitOfWork.Object)
            .Handle(new LogoutCommand(refreshToken), CancellationToken.None);
}
