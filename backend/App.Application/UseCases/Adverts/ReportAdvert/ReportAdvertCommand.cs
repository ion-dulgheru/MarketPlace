using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.ReportAdvert;

public record ReportAdvertCommand(Guid AdvertUuid, string Reason, Guid ReporterUuid) : ICommand;