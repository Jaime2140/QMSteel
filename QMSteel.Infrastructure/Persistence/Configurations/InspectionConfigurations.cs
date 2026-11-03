using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Welders;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
    {
        public void Configure(EntityTypeBuilder<Inspection> builder)
        {
            builder.Property(i => i.Folio).HasMaxLength(CodeLength);
            builder.Property(i => i.ProjectName).HasMaxLength(NameLength);
            builder.Property(i => i.ClientName).HasMaxLength(NameLength);
            builder.Property(i => i.PartNumber).HasMaxLength(CodeLength);
            builder.Property(i => i.AcceptanceCriteria).HasMaxLength(NameLength);
            builder.Property(i => i.CleanlinessStandard).HasMaxLength(CodeLength);
            builder.HasIndex(i => new { i.TenantId, i.Folio }).IsUnique();
            builder.HasIndex(i => i.InspectedOn);
            builder.HasIndex(i => i.ProjectName);

            builder.HasOne<Inspector>().WithMany().HasForeignKey(i => i.InspectorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Wps>().WithMany().HasForeignKey(i => i.WpsId).OnDelete(DeleteBehavior.Restrict);
            builder.ComplexProperty(i => i.SignedBy, s => s.ConfigureSignature());
            builder.PrimitiveCollection(i => i.EquipmentIds).HasField("_equipmentIds").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.PrimitiveCollection(i => i.PhotoFileIds).HasField("_photoFileIds").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(i => i.WeldItems).WithOne().HasForeignKey("InspectionId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.WeldItems).HasField("_weldItems").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();

            builder.HasMany(i => i.CoatingMeasurements).WithOne().HasForeignKey("InspectionId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.CoatingMeasurements).HasField("_coatingMeasurements").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class WeldInspectionItemConfiguration : IEntityTypeConfiguration<WeldInspectionItem>
    {
        public void Configure(EntityTypeBuilder<WeldInspectionItem> builder)
        {
            builder.Property(i => i.JointNumber).HasMaxLength(CodeLength);
            builder.Property(i => i.WelderStamp).HasMaxLength(CodeLength);
            builder.Property(i => i.Position).HasMaxLength(10);
            builder.Property(i => i.InspectedLengthMm).HasPrecision(12, 2);
            builder.Property(i => i.RejectedLengthMm).HasPrecision(12, 2);
            builder.HasOne<Welder>().WithMany().HasForeignKey(i => i.WelderId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    internal class CoatingMeasurementConfiguration : IEntityTypeConfiguration<CoatingMeasurement>
    {
        public void Configure(EntityTypeBuilder<CoatingMeasurement> builder)
        {
            builder.Property(m => m.Location).HasMaxLength(NameLength);
            builder.PrimitiveCollection(m => m.Readings).HasField("_readings").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
