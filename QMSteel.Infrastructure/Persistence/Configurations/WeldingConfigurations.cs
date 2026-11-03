using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Welders;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class WelderConfiguration : IEntityTypeConfiguration<Welder>
    {
        public void Configure(EntityTypeBuilder<Welder> builder)
        {
            builder.Property(w => w.EmployeeNumber).HasMaxLength(CodeLength);
            builder.Property(w => w.FullName).HasMaxLength(NameLength);
            builder.Property(w => w.StampNumber).HasMaxLength(CodeLength);
            builder.HasIndex(w => new { w.TenantId, w.StampNumber }).IsUnique();
            builder.HasIndex(w => new { w.TenantId, w.EmployeeNumber }).IsUnique();

            builder.HasMany(w => w.Qualifications).WithOne().HasForeignKey("WelderId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(w => w.Qualifications).HasField("_qualifications").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class WelderQualificationConfiguration : IEntityTypeConfiguration<WelderQualification>
    {
        public void Configure(EntityTypeBuilder<WelderQualification> builder)
        {
            builder.Property(q => q.TestPosition).HasMaxLength(10);
            builder.PrimitiveCollection(q => q.QualifiedPositions).HasField("_qualifiedPositions").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasOne<Wps>().WithMany().HasForeignKey(q => q.WpsId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    internal class WpsConfiguration : IEntityTypeConfiguration<Wps>
    {
        public void Configure(EntityTypeBuilder<Wps> builder)
        {
            builder.ToTable("Wps");
            builder.Property(w => w.Number).HasMaxLength(CodeLength);
            builder.HasIndex(w => new { w.TenantId, w.Number }).IsUnique();
            builder.OwnsOne(w => w.Variables, MapVariables);
            builder.ComplexProperty(w => w.ApprovedBy, s => s.ConfigureSignature());
            builder.PrimitiveCollection(w => w.SupportingPqrIds).HasField("_supportingPqrIds").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    internal class PqrConfiguration : IEntityTypeConfiguration<Pqr>
    {
        public void Configure(EntityTypeBuilder<Pqr> builder)
        {
            builder.Property(p => p.Number).HasMaxLength(CodeLength);
            builder.Property(p => p.TestingLaboratory).HasMaxLength(NameLength);
            builder.HasIndex(p => new { p.TenantId, p.Number }).IsUnique();
            builder.OwnsOne(p => p.Variables, MapVariables);
            builder.ComplexProperty(p => p.ApprovedBy, s => s.ConfigureSignature());
        }
    }
}
