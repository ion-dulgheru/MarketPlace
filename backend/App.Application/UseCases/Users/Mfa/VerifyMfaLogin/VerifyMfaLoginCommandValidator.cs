using FluentValidation;

namespace App.Application.UseCases.Users.Mfa.VerifyMfaLogin;

public class VerifyMfaLoginCommandValidator : AbstractValidator<VerifyMfaLoginCommand>
{
    public VerifyMfaLoginCommandValidator()
    {
        RuleFor(x => x.MfaToken).NotEmpty().WithMessage("MFA token is required.");
        RuleFor(x => x.Code).MfaCode();
    }
}
