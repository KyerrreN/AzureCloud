using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicMigrater.DAL.Entities;

namespace MusicMigrater.DAL.Configuration;

public class AudioConfiguration : IEntityTypeConfiguration<AudioEntity>
{
    public void Configure(EntityTypeBuilder<AudioEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Artist).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.Title).IsRequired();

        builder.HasIndex(x => new { x.ExternalId, x.Provider }).IsUnique();

        builder.HasIndex(x => x.CreatedAt);
    }
}
