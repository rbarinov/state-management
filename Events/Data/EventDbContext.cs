using Microsoft.EntityFrameworkCore;

namespace Events.Data;

public class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public required DbSet<StreamDto> Streams { get; set; }
    public required DbSet<EventDto> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var streamEntity = modelBuilder.Entity<StreamDto>();

        streamEntity
            .ToTable("streams")
            .HasKey(s => s.StreamId)
            .HasName("pk_streams");

        streamEntity.Property(s => s.StreamId)
            .HasColumnName("stream_id")
            .HasMaxLength(128)
            .IsRequired();

        streamEntity.Property(s => s.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        var eventEntity = modelBuilder.Entity<EventDto>();

        eventEntity
            .ToTable("events")
            .HasKey(e => e.GlobalVersion)
            .HasName("pk_events");

        eventEntity.Property(e => e.GlobalVersion)
            .UseIdentityColumn()
            .HasColumnName("global_version");

        eventEntity.HasIndex(e => new { e.StreamId, e.Version })
            .HasDatabaseName("ix_events_stream_id_version")
            .IsUnique();

        eventEntity.Property(e => e.StreamId)
            .HasColumnName("stream_id")
            .HasMaxLength(128)
            .IsRequired();

        eventEntity.HasOne(e => e.Stream)
            .WithMany(e => e.Events)
            .HasForeignKey(e => e.StreamId)
            .HasPrincipalKey(e => e.StreamId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_events_stream_id");

        eventEntity.HasIndex(e => e.StreamId)
            .HasDatabaseName("ix_events_stream_id");

        eventEntity.Property(e => e.Version)
            .HasColumnName("version")
            .IsRequired();

        eventEntity.Property(e => e.Type)
            .HasColumnName("type")
            .HasMaxLength(256)
            .IsRequired();

        eventEntity.Property(e => e.Payload)
            .HasColumnName("payload")
            .IsRequired();
    }
}
