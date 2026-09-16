using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserDetails> UserDetails => Set<UserDetails>();
    public DbSet<Advert> Adverts => Set<Advert>();
    public DbSet<AdvertPhoto> AdvertPhotos => Set<AdvertPhoto>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}