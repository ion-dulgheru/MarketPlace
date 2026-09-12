namespace App.Domain.Entites;

public class Adress
{
    public string Country {get; private set; }= null!;
    public string City {get; private set; }= null!;
    public string StreetAdress {get; private set; }= null!;
    public string Region {get; private set; }= null!;
    public string StreetNumber {get; private set; }= null!;
}