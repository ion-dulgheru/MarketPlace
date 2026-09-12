namespace App.Domain.Entities;
using App.Domain.Common;
public class User : BaseEntity {
    
    public string Email {get; private set; } = null!;
    public string PasswordHash {get ; private set; }=null!;
    public bool EmailVerification {get; private set; }

}