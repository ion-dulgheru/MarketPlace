using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace App.Persistence;

// Used by `dotnet ef` tooling (migrations, bundles) so design-time builds
// don't have to spin up the whole App.WebApi host — JWT certs, Key Vault
// and other runtime-only config aren't available at design time.
public class DataContextDesignTimeFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=design_time_only;Username=design_time_only;Password=design_time_only");
        return new DataContext(optionsBuilder.Options);
    }
}
