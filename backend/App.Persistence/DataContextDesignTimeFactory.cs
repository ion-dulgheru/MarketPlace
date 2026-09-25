using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace App.Persistence;

public class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    private const string AppWebApiUserSecretsId = "fbe9b283-2dc9-4ffb-afce-bb5be4763134";
    private const string PlaceholderConnectionString =
        "Host=localhost;Database=design_time_only;Username=design_time_only;Password=design_time_only";

    public DataContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(AppWebApiUserSecretsId)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default");

        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(string.IsNullOrWhiteSpace(connectionString)
            ? PlaceholderConnectionString
            : connectionString);

        return new DataContext(optionsBuilder.Options);
    }
}