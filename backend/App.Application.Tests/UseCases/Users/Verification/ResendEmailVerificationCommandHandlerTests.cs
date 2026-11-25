using App.Application.Abstractions.Email;
using App.Application.UseCases.Users.ResendEmailVerification;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Infrastructure.Auth;
using Moq;

namespace App.Application.Tests.UseCases.Users.Verification;

public class ResendEmailVerificationCommandHandlerTests
{
    private const string Email = "user@example.com";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IEmailSender> _emails = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshTokenGenerator _tokens = new();
    private readonly User _user = User.Create(Email, "hash");
    private string? _sentToken;

    public ResendEmailVerificationCommandHandlerTests()
    {
        _users.Setup(x => x.GetByEmailAsync(Email, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _emails
            .Setup(x => x.SendEmailVerificationAsync(Email, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((string _, string token, CancellationToken _) => _sentToken = token)
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WhenNotVerified_SendsNewTokenAndStoresOnlyItsHash()
    {
        _user.SetEmailVerificationToken("OLD", DateTime.UtcNow.AddHours(1));

        Assert.True((await Resend(Email)).IsSuccess);

        Assert.NotNull(_sentToken);
        Assert.Equal(_tokens.Hash(_sentToken), _user.EmailVerificationTokenHash);
        Assert.True(_user.EmailVerificationTokenExpiry > DateTime.UtcNow.AddHours(23));
    }

    [Fact]
    public async Task Handle_WhenSentLessThanAMinuteAgo_DoesNotSendAgain()
    {
        await Resend(Email);
        await Resend(Email);

        VerifySent(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenAlreadyVerified_SucceedsWithoutSending()
    {
        _user.VerifyEmail();

        Assert.True((await Resend(Email)).IsSuccess);
        VerifySent(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_SucceedsWithoutSending()
    {
        Assert.True((await Resend("nobody@example.com")).IsSuccess);
        VerifySent(Times.Never());
    }

    private Task<App.Domain.Shared.Result> Resend(string email) =>
        new ResendEmailVerificationCommandHandler(_users.Object, _tokens, _emails.Object, _unitOfWork.Object)
            .Handle(new ResendEmailVerificationCommand(email), CancellationToken.None);

    private void VerifySent(Times times) => _emails.Verify(
        x => x.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), times);
}
