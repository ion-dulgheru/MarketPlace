using FluentValidation;

namespace App.Application.UseCases.Users.Mfa.SetupMfa;

public class SetupMfaCommandValidator : AbstractValidator<SetupMfaCommand>
{
    public SetupMfaCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Password is required.");
    }
}
