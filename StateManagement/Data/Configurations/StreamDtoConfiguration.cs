using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StateManagement.Data.Entities;

namespace StateManagement.Data.Configurations;

public class StreamDtoConfiguration : IEntityTypeConfiguration<StreamDto>
{
    public void Configure(EntityTypeBuilder<StreamDto> builder)
    {
        builder
            .HasKey(s => s.StreamId);

        builder.Property(s => s.StreamId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(s => s.Version)
            .IsRequired()
            .IsConcurrencyToken();
    }
}
