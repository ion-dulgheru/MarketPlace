namespace App.Domain.Entities;

public class User{
    public long Id { get; set; }
    public Guid Guid {get; set; } =Guid.NewGuid() ;
    public bool IsActive {get; set;} =true;
    public DateTime CreatedDate {get; set; }=DateTime.UtcNow;
    public DateTime? UpdatedDate {get; set; } 
    public string Email {get;set;}=string.Empty;
    public string PasswordHash {get;set;}=string.Empty;
    public bool EmailVerification {get;set;}=false;



}