using App.Domain.Shared;

namespace App.Domain.Errors;

public static class AdvertReportErrors
{
    public static readonly Error InvalidReason =
        Error.Validation("AdvertReport.InvalidReason", "Reason must be one of: Spam, Fraud, Duplicate.");
}