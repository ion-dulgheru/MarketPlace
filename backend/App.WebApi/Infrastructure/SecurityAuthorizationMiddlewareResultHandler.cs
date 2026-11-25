using System.Security.Claims;
using App.Application.Abstractions.Interfaces;
using App.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace App.WebApi.Infrastructure;

public class SecurityAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            var auditLogger = context.RequestServices.GetService<ISecurityAuditLogger>();
            if (auditLogger is not null)
            {
                var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirstValue("sub");
                Guid.TryParse(sub, out var userGuid);
                var email = context.User.FindFirstValue(ClaimTypes.Email)
                    ?? context.User.FindFirstValue("email");

                await auditLogger.LogAsync(
                    SecurityEventType.AdminAccessDenied,
                    success: false,
                    userId: userGuid != Guid.Empty ? userGuid : null,
                    email: email,
                    details: $"Forbidden access to {context.Request.Path}",
                    ct: context.RequestAborted);
            }
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}

