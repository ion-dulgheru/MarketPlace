using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Gol deocamdată — aici intră mai târziu email, storage de fișiere etc.
        // (secț. 9.3 din ghid). Important e să existe metoda, ca structura
        // să fie completă de la început.

        return services;
    }
}
