using App.Domain.Enums;
using FluentValidation;

namespace App.Application.UseCases.Adverts.ReportAdvert;

public class ReportAdvertCommandValidator : AbstractValidator<ReportAdvertCommand>
{
        public ReportAdvertCommandValidator()
        {
            RuleFor(x => x.AdvertUuid).NotEqual(Guid.Empty);

            RuleFor(x => x.Reason)
                .Must(reason => Enum.TryParse<AdvertReportReason>(reason, ignoreCase: true, out _))
                .WithMessage("Reason must be one of: Spam, Fraud, Duplicate.");

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);
        }
}