using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistence.Configurations;

public class AdvertPhotoConfiguration : IEntityTypeConfiguration<AdvertPhoto>
{
    public void Configure(EntityTypeBuilder<AdvertPhoto> builder)
    {
        builder.ToTable("AdvertPhotos");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Guid).IsUnique();

        builder.Property(x => x.PhotoUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsPrimary)
            .IsRequired();
            
        builder.Property(x => x.CreatedDate)
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
    }
}
