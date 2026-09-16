using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistence.Configurations;

public class AdvertConfiguration : IEntityTypeConfiguration<Advert>
{
    public void Configure(EntityTypeBuilder<Advert> builder)
    {
        builder.ToTable("Adverts");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Guid).IsUnique();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SurfaceArea)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Rooms)
            .IsRequired();

        builder.Property(x => x.Floor)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.UserUuid)
            .IsRequired();

        builder.HasIndex(x => x.UserUuid);

        builder.Property(x => x.CreatedDate)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.ComplexProperty(x => x.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            addressBuilder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            addressBuilder.Property(a => a.Region)
                .IsRequired()
                .HasMaxLength(100);

            addressBuilder.Property(a => a.StreetAddress)
                .IsRequired()
                .HasMaxLength(200);

            addressBuilder.Property(a => a.StreetNumber)
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.HasMany(x => x.Photos)
            .WithOne(p => p.Advert)
            .HasForeignKey(p => p.AdvertId)
            .OnDelete(DeleteBehavior.Cascade);

        // IsActive = visibility filter, applied to all queries via the global query filter.
        // DeletedAt = audit-only timestamp (from ISoftDeletable), NOT used for filtering.
        builder.HasQueryFilter(x => x.IsActive);
    }
}
