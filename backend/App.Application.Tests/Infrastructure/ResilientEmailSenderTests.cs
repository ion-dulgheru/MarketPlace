using App.Application.Abstractions.Email;
using App.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Moq;

namespace App.Application.Tests.Infrastructure;

public class ResilientEmailSenderTests
{
    private const string Email = "user@example.com";
    private const string Token = "secret-token";

    private readonly Mock<IEmailSender> _inner = new();
    private readonly Mock<ILogger<ResilientEmailSender>> _logger = new();

    [Fact]
    public async Task Send_WhenProviderFails_DoesNotThrowAndLogsWithoutPersonalData()
    {
        _inner
            .Setup(x => x.SendEmailVerificationAsync(Email, Token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("quota exceeded"));
        _inner
            .Setup(x => x.SendPasswordResetEmailAsync(Email, Token, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("quota exceeded"));

        await Sender().SendEmailVerificationAsync(Email, Token);
        await Sender().SendPasswordResetEmailAsync(Email, Token);

        var messages = _logger.Invocations
            .Where(x => x.Method.Name == nameof(ILogger.Log))
            .Select(x => x.Arguments[2].ToString()!)
            .ToList();
        Assert.Equal(2, messages.Count);
        Assert.All(messages, message =>
        {
            Assert.DoesNotContain(Email, message);
            Assert.DoesNotContain(Token, message);
        });
    }

    [Fact]
    public async Task Send_WhenProviderWorks_ForwardsTheCall()
    {
        await Sender().SendEmailVerificationAsync(Email, Token);

        _inner.Verify(x => x.SendEmailVerificationAsync(Email, Token, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Empty(_logger.Invocations);
    }

    [Fact]
    public async Task Send_WhenRequestIsCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _inner
            .Setup(x => x.SendEmailVerificationAsync(Email, Token, cts.Token))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => Sender().SendEmailVerificationAsync(Email, Token, cts.Token));
    }

    private ResilientEmailSender Sender() => new(_inner.Object, _logger.Object);
}
