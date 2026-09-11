namespace App.Domain.Entities;
using App.Domain.Common;
public class User : BaseEntity {
    
    public string Email {get; private set; }
    public string PasswordHash {get ; private set; }
    public bool EmailVerification {get; private set; }

}