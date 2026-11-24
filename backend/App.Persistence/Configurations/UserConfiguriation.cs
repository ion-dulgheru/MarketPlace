using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Two parallel requests with the same TOTP code both read the old step; the concurrency
        // check makes the second save fail, so one code can never produce two sessions.
        builder.Property(x => x.LastTotpStep).IsConcurrencyToken();

        builder.Property(x => x.MfaTokenHash).HasMaxLength(64);
    }
}
