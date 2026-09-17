namespace App.Application.Abstractions.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitize(string html);
}
