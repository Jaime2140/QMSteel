using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Suppliers;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.Property(e => e.Code).HasMaxLength(CodeLength);
            builder.Property(e => e.Name).HasMaxLength(NameLength);
            builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();

            builder.HasMany(e => e.Calibrations).WithOne().HasForeignKey("EquipmentId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(e => e.Calibrations).HasField("_calibrations").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();

            builder.HasMany(e => e.Verifications).WithOne().HasForeignKey("EquipmentId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(e => e.Verifications).HasField("_verifications").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class EquipmentCalibrationConfiguration : IEntityTypeConfiguration<EquipmentCalibration>
    {
        public void Configure(EntityTypeBuilder<EquipmentCalibration> builder)
        {
            builder.Property(c => c.LaboratoryName).HasMaxLength(NameLength);
            builder.Property(c => c.CertificateNumber).HasMaxLength(CodeLength);
            builder.HasOne<Supplier>().WithMany().HasForeignKey(c => c.SupplierId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    internal class EquipmentVerificationConfiguration : IEntityTypeConfiguration<EquipmentVerification>
    {
        public void Configure(EntityTypeBuilder<EquipmentVerification> builder)
        {
            builder.ComplexProperty(v => v.PerformedBy, s => s.ConfigureSignature());
        }
    }
}
