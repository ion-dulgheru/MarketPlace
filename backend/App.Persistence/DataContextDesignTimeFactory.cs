using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace App.Persistence;

// Used by `dotnet ef` tooling (migrations, bundles) so design-time builds
// don't have to spin up the whole App.WebApi host — JWT certs, Key Vault
// and other runtime-only config aren't available at design time.
//
// `migrations add` only needs the model, so the placeholder connection string
// below is enough. `database update` needs a real connection: it's read from
// App.WebApi's user-secrets (UserSecretsId below) if present, so the real
// connection string never has to be typed on the command line.
public class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    private const string AppWebApiUserSecretsId = "fbe9b283-2dc9-4ffb-afce-bb5be4763134";
    private const string PlaceholderConnectionString =
        "Host=localhost;Database=design_time_only;Username=design_time_only;Password=design_time_only";

    public DataContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(AppWebApiUserSecretsId)
            .Build();

        var connectionString = configuration.GetConnectionString("Default");

        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(string.IsNullOrWhiteSpace(connectionString)
            ? PlaceholderConnectionString
            : connectionString);
        return new DataContext(optionsBuilder.Options);
    }
}
