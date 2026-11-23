using App.Application.Abstractions.Interfaces;
using App.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Services;

/// <summary>
/// Temporary <see cref="ISecurityAuditLogger"/> that writes structured log entries only.
/// To be replaced by the database-backed implementation (SecurityAuditLogs table).
/// </summary>
public sealed class LoggingSecurityAuditLogger(ILogger<LoggingSecurityAuditLogger> logger) : ISecurityAuditLogger
{
    public Task LogAsync(
        SecurityEventType eventType,
        bool success,
        Guid? userId = null,
        string? email = null,
        string? details = null,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "SecurityEvent {EventType} Success={Success} UserId={UserId} Email={Email} Details={Details}",
            eventType, success, userId, email, details);

        return Task.CompletedTask;
    }
}
