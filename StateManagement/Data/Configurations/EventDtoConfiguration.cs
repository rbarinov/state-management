using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StateManagement.Data.Entities;

namespace StateManagement.Data.Configurations;

public class EventDtoConfiguration : IEntityTypeConfiguration<EventDto>
{
    public void Configure(EntityTypeBuilder<EventDto> builder)
    {
        builder
            .HasKey(e => e.GlobalVersion);

        builder.Property(e => e.GlobalVersion)
            .UseIdentityColumn();

        builder.HasIndex(e => new { e.StreamId, e.Version })
            .IsUnique();

        builder.Property(e => e.StreamId)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasOne(e => e.Stream)
            .WithMany(e => e.Events)
            .HasForeignKey(e => e.StreamId)
            .HasPrincipalKey(e => e.StreamId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.StreamId);

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.EventAt)
            .IsRequired();

        builder.Property(e => e.Payload)
            .IsRequired();
    }
}
