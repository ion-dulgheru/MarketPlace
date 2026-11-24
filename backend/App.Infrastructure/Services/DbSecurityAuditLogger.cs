using App.Application.Abstractions.Interfaces;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Services;

public sealed class DbSecurityAuditLogger(
    IServiceScopeFactory scopeFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<DbSecurityAuditLogger> logger) : ISecurityAuditLogger
{
    public async Task LogAsync(
        SecurityEventType eventType,
        bool success,
        Guid? userId = null,
        string? email = null,
        string? details = null,
        CancellationToken ct = default)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request?.Headers.UserAgent.ToString();

            using var scope = scopeFactory.CreateScope();
            var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            var entry = SecurityAuditLog.Create(
                eventType,
                success,
                userId,
                email,
                ipAddress,
                userAgent,
                details);

            await dataContext.SecurityAuditLogs.AddAsync(entry, ct);
            await dataContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record security audit log for {EventType}", eventType);
        }
    }
}

