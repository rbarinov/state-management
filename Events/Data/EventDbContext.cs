using Microsoft.EntityFrameworkCore;

namespace Events.Data;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options)
        : base(options)
    {
    }

    public required DbSet<StreamDto> Streams { get; set; }
    public required DbSet<EventDto> Events { get; set; }
    public required DbSet<StateDto> States { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var streamEntity = modelBuilder.Entity<StreamDto>();

        streamEntity
            .HasKey(s => s.StreamId);

        streamEntity.Property(s => s.StreamId)
            .HasMaxLength(256)
            .IsRequired();

        streamEntity.Property(s => s.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        var eventEntity = modelBuilder.Entity<EventDto>();

        eventEntity
            .HasKey(e => e.GlobalVersion);

        eventEntity.Property(e => e.GlobalVersion)
            .UseIdentityColumn();

        eventEntity.HasIndex(e => new { e.StreamId, e.Version })
            .IsUnique();

        eventEntity.Property(e => e.StreamId)
            .HasMaxLength(256)
            .IsRequired();

        eventEntity.HasOne(e => e.Stream)
            .WithMany(e => e.Events)
            .HasForeignKey(e => e.StreamId)
            .HasPrincipalKey(e => e.StreamId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        eventEntity.HasIndex(e => e.StreamId);

        eventEntity.Property(e => e.Version)
            .IsRequired();

        eventEntity.Property(e => e.Type)
            .HasMaxLength(256)
            .IsRequired();

        eventEntity.Property(e => e.EventAt)
            .IsRequired();

        eventEntity.Property(e => e.Payload)
            .IsRequired();

        var stateEntity = modelBuilder.Entity<StateDto>();

        stateEntity.HasKey(e => e.Key);

        stateEntity.Property(e => e.Key)
            .HasMaxLength(256)
            .IsRequired();

        stateEntity.Property(e => e.UpdatedAt)
            .IsRequired();

        stateEntity.Property(e => e.Payload)
            .IsRequired();
    }
}
