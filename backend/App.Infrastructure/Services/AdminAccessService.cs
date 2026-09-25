using App.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace App.Infrastructure.Services;

public class AdminAccessService(IConfiguration configuration) : IAdminAccessService
{
    public bool IsAdmin(string email)
    {
        var adminEmails = configuration.GetSection("Admin:Emails").Get<string[]>() ?? [];
        return adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
}
