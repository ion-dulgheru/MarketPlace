using App.Domain.Common;
using App.Domain.Enums;

namespace App.Domain.Entities;

public class AdvertReport : BaseEntity
{
    private AdvertReport() { }

    public long AdvertId { get; private set; }
    public Guid ReporterUuid { get; private set; }
    public AdvertReportReason Reason { get; private set; }

    public static AdvertReport Create(long advertId, Guid reporterUuid, AdvertReportReason reason)
    {
        return new AdvertReport
        {
            AdvertId = advertId,
            ReporterUuid = reporterUuid,
            Reason = reason
        };
    }
}