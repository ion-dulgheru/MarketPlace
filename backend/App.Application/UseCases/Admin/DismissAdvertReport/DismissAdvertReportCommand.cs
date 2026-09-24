using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Admin.DismissAdvertReport;

public record DismissAdvertReportCommand(Guid ReportUuid) : ICommand;