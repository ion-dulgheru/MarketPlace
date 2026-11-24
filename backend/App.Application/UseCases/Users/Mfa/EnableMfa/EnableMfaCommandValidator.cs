using FluentValidation;

namespace App.Application.UseCases.Users.Mfa.EnableMfa;

public class EnableMfaCommandValidator : AbstractValidator<EnableMfaCommand>
{
    public EnableMfaCommandValidator()
    {
        RuleFor(x => x.Code).MfaCode();
    }
}
