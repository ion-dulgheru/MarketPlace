using App.Domain.Enums;

namespace App.Domain.Entities;

public class SecurityAuditLog
{
    public long Id { get; private set; }
    public SecurityEventType EventType { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Email { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool Success { get; private set; }
    public string? Details { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private SecurityAuditLog() { }

    public static SecurityAuditLog Create(
        SecurityEventType eventType,
        bool success,
        Guid? userId = null,
        string? email = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null)
    {
        return new SecurityAuditLog
        {
            EventType = eventType,
            Success = success,
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent != null && userAgent.Length > 256 ? userAgent[..256] : userAgent,
            Details = details != null && details.Length > 500 ? details[..500] : details,
            CreatedAt = DateTime.UtcNow
        };
    }
}

