namespace App.Domain.Entites

public class Users : BaseEntity {
    public string Email {get; private set; }
    public string PasswordHash {get; private set; }
    public bool EmailVerification {get; private set; }
}
