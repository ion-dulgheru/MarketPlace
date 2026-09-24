using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace App.Persistence;

// Used by `dotnet ef` tooling (migrations, bundles) so design-time builds
// don't have to spin up the whole App.WebApi host — JWT certs, Key Vault
// and other runtime-only config aren't available at design time.
public class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    private const string UserSecretsId = "fbe9b283-2dc9-4ffb-afce-bb5be4763134";

    public DataContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables();

        var secretsPath = GetUserSecretsPath();
        if (File.Exists(secretsPath))
        {
            configurationBuilder.AddJsonFile(secretsPath, optional: true);
        }

        var configuration = configurationBuilder.Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Database=design_time_only;Username=design_time_only;Password=design_time_only";

        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DataContext(optionsBuilder.Options);
    }

    private static string GetUserSecretsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Microsoft", "UserSecrets", UserSecretsId, "secrets.json");
    }
}