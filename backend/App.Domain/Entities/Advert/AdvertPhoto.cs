namespace App.Domain.Entites;

public class AdvertPhoto 
{
    public string PhotoUrl {get; private set; }= null!;
    public string FileName {get; private set; }= null!;
    public string ContentType {get; private set; }= null!;
    public bool IsPrimary {get; private set; }
    public long AdvertId {get; private set;}
}