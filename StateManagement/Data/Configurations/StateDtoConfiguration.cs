using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StateManagement.Data.Entities;

namespace StateManagement.Data.Configurations;

public class StateDtoConfiguration : IEntityTypeConfiguration<StateDto>
{
    public void Configure(EntityTypeBuilder<StateDto> builder)
    {
        builder.HasKey(e => e.Key);

        builder.Property(e => e.Key)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(e => e.Payload)
            .IsRequired();
    }
}
