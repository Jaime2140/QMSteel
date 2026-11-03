using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Documents;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class ControlledDocumentConfiguration : IEntityTypeConfiguration<ControlledDocument>
    {
        public void Configure(EntityTypeBuilder<ControlledDocument> builder)
        {
            builder.Property(d => d.Code).HasMaxLength(CodeLength);
            builder.Property(d => d.Title).HasMaxLength(NameLength);
            builder.HasIndex(d => new { d.TenantId, d.Code }).IsUnique();
            builder.PrimitiveCollection(d => d.StandardClauses).HasField("_standardClauses").UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(d => d.Revisions).WithOne().HasForeignKey("DocumentId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(d => d.Revisions).HasField("_revisions").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();

            builder.HasMany(d => d.Copies).WithOne().HasForeignKey("DocumentId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(d => d.Copies).HasField("_copies").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class DocumentRevisionConfiguration : IEntityTypeConfiguration<DocumentRevision>
    {
        public void Configure(EntityTypeBuilder<DocumentRevision> builder)
        {
            builder.ComplexProperty(r => r.PreparedBy, s => s.ConfigureSignature());
            builder.ComplexProperty(r => r.ReviewedBy, s => s.ConfigureSignature());
            builder.ComplexProperty(r => r.ApprovedBy, s => s.ConfigureSignature());
            builder.HasIndex("DocumentId", nameof(DocumentRevision.Number)).IsUnique();
        }
    }

    internal class ControlledCopyConfiguration : IEntityTypeConfiguration<ControlledCopy>
    {
        public void Configure(EntityTypeBuilder<ControlledCopy> builder)
        {
            builder.Property(c => c.RecipientName).HasMaxLength(NameLength);
            builder.Property(c => c.RecipientEmail).HasMaxLength(NameLength);
            builder.HasIndex(c => c.RevisionId);
        }
    }
}
