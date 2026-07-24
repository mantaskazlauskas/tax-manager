using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxManager.Domain.Entities;

namespace TaxManager.Infrastructure.Persistence.Configurations;

public class TaxRecordConfiguration : IEntityTypeConfiguration<TaxRecord>
{
    public void Configure(EntityTypeBuilder<TaxRecord> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PeriodType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Rate)
            .HasPrecision(9, 4)
            .IsRequired();

        builder.HasIndex(t => new { t.MunicipalityId, t.PeriodType });
    }
}
