namespace App.Application.Abstractions.Interfaces;

public interface IAdminAccessService
{
    bool IsAdmin(string email);
}
