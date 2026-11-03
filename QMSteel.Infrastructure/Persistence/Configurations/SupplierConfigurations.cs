using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.Property(s => s.Name).HasMaxLength(NameLength);
            builder.Property(s => s.ContactName).HasMaxLength(NameLength);
            builder.Property(s => s.ContactEmail).HasMaxLength(NameLength);
            builder.Property(s => s.ContactPhone).HasMaxLength(CodeLength);
            builder.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();
            builder.PrimitiveCollection(s => s.Categories)
                .HasField("_categories")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .ElementType(e => e.HasConversion<string>());

            builder.HasMany(s => s.Evaluations).WithOne().HasForeignKey("SupplierId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(s => s.Evaluations).HasField("_evaluations").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class SupplierEvaluationConfiguration : IEntityTypeConfiguration<SupplierEvaluation>
    {
        public void Configure(EntityTypeBuilder<SupplierEvaluation> builder)
        {
            builder.Property(e => e.ScorePercent).HasPrecision(5, 2);
            builder.ComplexProperty(e => e.EvaluatedBy, s => s.ConfigureSignature());
        }
    }

    internal class ReceivingInspectionConfiguration : IEntityTypeConfiguration<ReceivingInspection>
    {
        public void Configure(EntityTypeBuilder<ReceivingInspection> builder)
        {
            builder.Property(r => r.Folio).HasMaxLength(CodeLength);
            builder.Property(r => r.PurchaseOrder).HasMaxLength(CodeLength);
            builder.Property(r => r.DeliveryNote).HasMaxLength(CodeLength);
            builder.Property(r => r.InvoiceNumber).HasMaxLength(CodeLength);
            builder.HasIndex(r => new { r.TenantId, r.Folio }).IsUnique();
            builder.HasOne<Supplier>().WithMany().HasForeignKey(r => r.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.ComplexProperty(r => r.InspectedBy, s => s.ConfigureSignature());

            builder.HasMany(r => r.Items).WithOne().HasForeignKey("ReceivingInspectionId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(r => r.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class ReceivedItemConfiguration : IEntityTypeConfiguration<ReceivedItem>
    {
        public void Configure(EntityTypeBuilder<ReceivedItem> builder)
        {
            builder.Property(i => i.HeatNumber).HasMaxLength(CodeLength);
            builder.Property(i => i.Unit).HasMaxLength(20);
            builder.HasIndex(i => i.HeatNumber);
            builder.PrimitiveCollection(i => i.Defects)
                .HasField("_defects")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .ElementType(e => e.HasConversion<string>());
        }
    }
}
