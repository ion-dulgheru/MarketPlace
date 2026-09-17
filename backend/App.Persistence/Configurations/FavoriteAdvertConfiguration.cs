using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistence.Configurations;

public class FavoriteAdvertConfiguration : IEntityTypeConfiguration<FavoriteAdvert>
{
    public void Configure(EntityTypeBuilder<FavoriteAdvert> builder)
    {
        builder.ToTable("FavoriteAdverts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserUuid)
            .IsRequired();

        builder.Property(x => x.AdvertId)
            .IsRequired();

        builder.HasOne(x => x.Advert)
            .WithMany()
            .HasForeignKey(x => x.AdvertId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserUuid, x.AdvertId })
            .IsUnique();

        builder.HasIndex(x => x.UserUuid);

        builder.Property(x => x.CreatedDate)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasQueryFilter(x => x.Advert.IsActive);
    }
}
