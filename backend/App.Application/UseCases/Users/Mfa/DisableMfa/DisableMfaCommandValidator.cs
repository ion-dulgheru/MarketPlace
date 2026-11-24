using FluentValidation;

namespace App.Application.UseCases.Users.Mfa.DisableMfa;

public class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    public DisableMfaCommandValidator()
    {
        RuleFor(x => x.Code).MfaCode();
    }
}
