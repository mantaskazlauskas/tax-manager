using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxManager.Domain.Entities;

namespace TaxManager.Infrastructure.Persistence.Configurations;

public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Assumption: municipality names are unique and looked up case-insensitively.
        builder.HasIndex(m => m.Name).IsUnique();

        builder.HasMany(m => m.TaxRecords)
            .WithOne()
            .HasForeignKey(t => t.MunicipalityId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaxRecords exposes a read-only wrapper, so EF must read/write the backing field directly.
        builder.Navigation(m => m.TaxRecords).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
