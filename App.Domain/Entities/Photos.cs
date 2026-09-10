namespace App.Domain.Entites;

public class Photo 
{
    public string PhotoUrl {get; private set; }
    public string FileName {get; private set; }
    public bool IsPrimary {get; private set; }
    public long AdvertId {get; private set;}
}