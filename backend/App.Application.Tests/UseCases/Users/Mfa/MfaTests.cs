using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Mfa;
using App.Application.UseCases.Users.Mfa;
using App.Application.UseCases.Users.Mfa.DisableMfa;
using App.Application.UseCases.Users.Mfa.EnableMfa;
using App.Application.UseCases.Users.Mfa.SetupMfa;
using App.Application.UseCases.Users.Mfa.VerifyMfaLogin;
using App.Application.UseCases.Users.SignIn;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Infrastructure.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OtpNet;

namespace App.Application.Tests.UseCases.Users.Mfa;

public class MfaTests
{
    private const string Email = "user@example.com";
    private const string Password = "Passw0rd!";
    private const string MfaToken = "mfa-token";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IUserSessionRepository> _sessions = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();
    private readonly Mock<ISecurityAuditLogger> _audit = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly RefreshTokenGenerator _tokens = new();
    private readonly TotpService _totp = new();
    private readonly TotpSecretProtector _protector = new(
        new EphemeralDataProtectionProvider(), NullLogger<TotpSecretProtector>.Instance);

    private readonly User _user = User.Create(Email, BCrypt.Net.BCrypt.HashPassword(Password, workFactor: 4));
    private readonly string _secret;

    public MfaTests()
    {
        _secret = _totp.GenerateSecret();
        _user.VerifyEmail();

        _users.Setup(x => x.GetByUuidAsync(_user.Guid, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _users.Setup(x => x.GetByEmailAsync(Email, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _users
            .Setup(x => x.GetByMfaTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string hash, CancellationToken _) => _user.MfaTokenHash == hash ? _user : null);
        _jwt.Setup(x => x.GenerateToken(_user)).Returns(("access-token", "jwt-id"));
    }

    // ---- Login, step 1 ----

    [Fact]
    public async Task SignIn_WhenMfaEnabled_ReturnsMfaTokenAndNoTokens()
    {
        EnableMfa();

        var result = await SignIn();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.MfaRequired);
        Assert.Null(result.Value.AccessToken);
        Assert.Null(result.Value.RefreshToken);
        Assert.Equal(_tokens.Hash(result.Value.MfaToken!), _user.MfaTokenHash);
        Assert.NotEqual(result.Value.MfaToken, _user.MfaTokenHash);
        VerifyNoSession();
    }

    [Fact]
    public async Task SignIn_WhenMfaSetupNotConfirmed_ReturnsTokens()
    {
        _user.StartMfaSetup(_protector.Protect(_secret));

        var result = await SignIn();

        Assert.False(result.Value.MfaRequired);
        Assert.Equal("access-token", result.Value.AccessToken);
    }

    [Fact]
    public async Task SignIn_WhenMfaEnabled_DoesNotResetFailedAttempts()
    {
        EnableMfa();
        _user.RegisterFailedLogin(DateTime.UtcNow);

        await SignIn();

        Assert.Equal(1, _user.FailedLoginAttempts);
    }

    // ---- Login, step 2 ----

    [Fact]
    public async Task VerifyLogin_WithCorrectCode_CreatesSessionAndResetsFailedAttempts()
    {
        EnableMfa();
        StartLogin();
        _user.RegisterFailedLogin(DateTime.UtcNow);

        var result = await VerifyLogin(CurrentCode());

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.False(result.Value.MfaRequired);
        Assert.Equal(0, _user.FailedLoginAttempts);
        Assert.Null(_user.MfaTokenHash);
        _sessions.Verify(x => x.AddAsync(
            It.Is<UserSession>(s => s.RefreshTokenHash == _tokens.Hash(result.Value.RefreshToken!)),
            It.IsAny<CancellationToken>()), Times.Once);
        VerifyAudit(SecurityEventType.LoginSucceeded, Times.Once());
    }

    [Fact]
    public async Task VerifyLogin_WithWrongCode_FailsAndCountsAttempt()
    {
        EnableMfa();
        StartLogin();

        var result = await VerifyLogin(WrongCode());

        Assert.Equal(MfaErrors.InvalidCode, result.Error);
        Assert.Equal(1, _user.FailedLoginAttempts);
        VerifyNoSession();
        VerifyAudit(SecurityEventType.MfaFailed, Times.Once());
    }

    [Fact]
    public async Task VerifyLogin_WithExpiredMfaToken_Fails()
    {
        EnableMfa();
        _user.SetMfaToken(_tokens.Hash(MfaToken), DateTime.UtcNow.AddSeconds(-1));

        var result = await VerifyLogin(CurrentCode());

        Assert.Equal(MfaErrors.InvalidLoginToken, result.Error);
        VerifyNoSession();
    }

    [Fact]
    public async Task VerifyLogin_WithUnknownMfaToken_Fails()
    {
        EnableMfa();
        StartLogin();

        var result = await LoginHandler().Handle(
            new VerifyMfaLoginCommand("other-token", CurrentCode()), CancellationToken.None);

        Assert.Equal(MfaErrors.InvalidLoginToken, result.Error);
    }

    [Fact]
    public async Task VerifyLogin_WithSameMfaTokenTwice_Fails()
    {
        EnableMfa();
        StartLogin();
        await VerifyLogin(CurrentCode());

        var result = await VerifyLogin(CurrentCode());

        Assert.Equal(MfaErrors.InvalidLoginToken, result.Error);
    }

    [Fact]
    public async Task VerifyLogin_WithReusedCode_Fails()
    {
        EnableMfa();
        var code = CurrentCode();
        StartLogin();
        Assert.True((await VerifyLogin(code)).IsSuccess);

        StartLogin();
        var result = await VerifyLogin(code);

        Assert.Equal(MfaErrors.InvalidCode, result.Error);
        Assert.Equal(1, _user.FailedLoginAttempts);
    }

    [Fact]
    public async Task VerifyLogin_AfterFiveWrongCodes_LocksAccountAndRejectsCorrectCode()
    {
        EnableMfa();
        StartLogin();

        for (var i = 0; i < 5; i++)
        {
            await VerifyLogin(WrongCode());
        }

        Assert.True(_user.IsLockedOut(DateTime.UtcNow));
        Assert.Null(_user.MfaTokenHash);
        VerifyAudit(SecurityEventType.AccountLocked, Times.Once());

        StartLogin();
        Assert.Equal(MfaErrors.AccountLocked, (await VerifyLogin(CurrentCode())).Error);
        VerifyNoSession();
    }

    [Fact]
    public async Task VerifyLogin_WhenSecretCannotBeDecrypted_FailsWithoutCountingAttempt()
    {
        var otherKeys = new TotpSecretProtector(
            new EphemeralDataProtectionProvider(), NullLogger<TotpSecretProtector>.Instance);
        _user.StartMfaSetup(otherKeys.Protect(_secret));
        _user.EnableMfa(0);
        StartLogin();

        var result = await VerifyLogin(CurrentCode());

        Assert.Equal(MfaErrors.SecretUnavailable, result.Error);
        Assert.Equal(0, _user.FailedLoginAttempts);
    }

    [Fact]
    public void ResetPassword_InvalidatesPendingMfaLogin()
    {
        EnableMfa();
        StartLogin();

        _user.ResetPassword("new-hash");

        Assert.False(_user.IsMfaTokenValid(DateTime.UtcNow));
        Assert.True(_user.TwoFactorEnabled);
    }

    // ---- Setup / enable / disable ----

    [Fact]
    public async Task Setup_StoresEncryptedSecretWithoutEnablingMfa()
    {
        var result = await SetupHandler().Handle(new SetupMfaCommand(_user.Guid), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(_user.TwoFactorEnabled);
        Assert.NotEqual(result.Value.Secret, _user.TotpSecret);
        Assert.Equal(result.Value.Secret, _protector.Unprotect(_user.TotpSecret!));
        Assert.Equal(
            $"otpauth://totp/OpenKey:user%40example.com?secret={result.Value.Secret}&issuer=OpenKey",
            result.Value.OtpAuthUri);
    }

    [Fact]
    public async Task Setup_WhenAlreadyEnabled_FailsAndKeepsSecret()
    {
        EnableMfa();
        var storedSecret = _user.TotpSecret;

        var result = await SetupHandler().Handle(new SetupMfaCommand(_user.Guid), CancellationToken.None);

        Assert.Equal(MfaErrors.AlreadyEnabled, result.Error);
        Assert.Equal(storedSecret, _user.TotpSecret);
    }

    [Fact]
    public async Task Enable_WithCorrectCode_EnablesMfaAndBurnsTheCode()
    {
        _user.StartMfaSetup(_protector.Protect(_secret));
        var code = CurrentCode();

        var result = await Enable(code);

        Assert.True(result.IsSuccess);
        Assert.True(_user.TwoFactorEnabled);
        VerifyAudit(SecurityEventType.MfaEnabled, Times.Once());

        StartLogin();
        Assert.Equal(MfaErrors.InvalidCode, (await VerifyLogin(code)).Error);
    }

    [Fact]
    public async Task Enable_WithWrongCode_DoesNotEnableMfa()
    {
        _user.StartMfaSetup(_protector.Protect(_secret));

        var result = await Enable(WrongCode());

        Assert.Equal(MfaErrors.InvalidCode, result.Error);
        Assert.False(_user.TwoFactorEnabled);
    }

    [Fact]
    public async Task Enable_WithoutSetup_Fails()
    {
        Assert.Equal(MfaErrors.SetupNotStarted, (await Enable("123456")).Error);
    }

    [Fact]
    public async Task Disable_WithCorrectCode_RemovesSecret()
    {
        EnableMfa();

        var result = await Disable(CurrentCode());

        Assert.True(result.IsSuccess);
        Assert.False(_user.TwoFactorEnabled);
        Assert.Null(_user.TotpSecret);
        VerifyAudit(SecurityEventType.MfaDisabled, Times.Once());
    }

    [Fact]
    public async Task Disable_WithWrongCode_KeepsMfaAndCountsAttempt()
    {
        EnableMfa();

        var result = await Disable(WrongCode());

        Assert.Equal(MfaErrors.InvalidCode, result.Error);
        Assert.True(_user.TwoFactorEnabled);
        Assert.Equal(1, _user.FailedLoginAttempts);
        VerifyAudit(SecurityEventType.MfaFailed, Times.Once());
    }

    [Theory]
    [InlineData("123456", true)]
    [InlineData("12345", false)]
    [InlineData("1234567", false)]
    [InlineData("12a456", false)]
    [InlineData("123456\n", false)]
    [InlineData("١٢٣٤٥٦", false)]
    [InlineData("", false)]
    public void CodeValidation_AcceptsExactlySixAsciiDigits(string code, bool valid)
    {
        Assert.Equal(valid, new EnableMfaCommandValidator().Validate(new EnableMfaCommand(_user.Guid, code)).IsValid);
    }

    // ---- Helpers ----

    private void EnableMfa()
    {
        _user.StartMfaSetup(_protector.Protect(_secret));
        _user.EnableMfa(0);
    }

    private void StartLogin() => _user.SetMfaToken(_tokens.Hash(MfaToken), DateTime.UtcNow.AddMinutes(5));

    private string CurrentCode() => new Totp(Base32Encoding.ToBytes(_secret)).ComputeTotp();

    private string WrongCode()
    {
        // Any code outside the accepted window (previous, current and next step; one extra step each
        // side in case the clock crosses a step boundary during the test).
        var totp = new Totp(Base32Encoding.ToBytes(_secret));
        var accepted = Enumerable.Range(-2, 5)
            .Select(i => totp.ComputeTotp(DateTime.UtcNow.AddSeconds(i * 30)))
            .ToHashSet();
        return Enumerable.Range(0, 10).Select(i => i.ToString("D6")).First(code => !accepted.Contains(code));
    }

    private MfaCodeVerifier Verifier() => new(_totp, _protector);

    private MfaFailureRecorder Recorder() => new(_audit.Object, _unitOfWork.Object);

    private SetupMfaCommandHandler SetupHandler() => new(_users.Object, _totp, _protector, _unitOfWork.Object);

    private VerifyMfaLoginCommandHandler LoginHandler() => new(
        _users.Object, _sessions.Object, _jwt.Object, _tokens, Verifier(), Recorder(), _audit.Object, _unitOfWork.Object);

    private Task<Domain.Shared.Result<SignInResponse>> SignIn() =>
        new SignInCommandHandler(_users.Object, _sessions.Object, _jwt.Object, _tokens, _unitOfWork.Object)
            .Handle(new SignInCommand(Email, Password), CancellationToken.None);

    private Task<Domain.Shared.Result<SignInResponse>> VerifyLogin(string code) =>
        LoginHandler().Handle(new VerifyMfaLoginCommand(MfaToken, code), CancellationToken.None);

    private Task<Domain.Shared.Result> Enable(string code) =>
        new EnableMfaCommandHandler(_users.Object, Verifier(), _audit.Object, _unitOfWork.Object)
            .Handle(new EnableMfaCommand(_user.Guid, code), CancellationToken.None);

    private Task<Domain.Shared.Result> Disable(string code) =>
        new DisableMfaCommandHandler(_users.Object, Verifier(), Recorder(), _audit.Object, _unitOfWork.Object)
            .Handle(new DisableMfaCommand(_user.Guid, code), CancellationToken.None);

    private void VerifyNoSession() =>
        _sessions.Verify(x => x.AddAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Never);

    private void VerifyAudit(SecurityEventType eventType, Times times) =>
        _audit.Verify(x => x.LogAsync(
            eventType, It.IsAny<bool>(), _user.Guid, Email, It.IsAny<string?>(), It.IsAny<CancellationToken>()), times);
}
