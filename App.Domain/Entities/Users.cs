namespace App.Domain.Entites

public class Users : BaseEntity {
    public string Email {get; private get; }
    public string PasswordHash {get; private get;}
    
}