using FluentValidation;

namespace App.Application.UseCases.Users.Mfa;

internal static class MfaCodeRules
{
    // [0-9], not \d: \d also matches non-ASCII digits. \z, not $: $ allows a trailing newline.
    public static IRuleBuilderOptions<T, string> MfaCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Code is required.")
            .Matches(@"^[0-9]{6}\z").WithMessage("Code must have 6 digits.");
}
