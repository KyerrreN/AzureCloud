using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicMigrater.DAL.Entities;

namespace MusicMigrater.DAL.Configuration;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
    public void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();

        // no encryption for now, since its not a multiuser application
        // but if it will be, it should be encrypted
        builder.Property(x => x.Value).IsRequired();
    }
}
