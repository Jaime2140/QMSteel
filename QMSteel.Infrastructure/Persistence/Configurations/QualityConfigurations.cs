using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Suppliers;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class NonConformityConfiguration : IEntityTypeConfiguration<NonConformity>
    {
        public void Configure(EntityTypeBuilder<NonConformity> builder)
        {
            builder.Property(n => n.Folio).HasMaxLength(CodeLength);
            builder.Property(n => n.ProjectName).HasMaxLength(NameLength);
            builder.Property(n => n.ClientName).HasMaxLength(NameLength);
            builder.Property(n => n.PartNumber).HasMaxLength(CodeLength);
            builder.HasIndex(n => new { n.TenantId, n.Folio }).IsUnique();
            builder.HasIndex(n => n.Status);
            builder.HasIndex(n => n.SourceReferenceId);
            builder.HasOne<Supplier>().WithMany().HasForeignKey(n => n.SupplierId).OnDelete(DeleteBehavior.Restrict);

            builder.PrimitiveCollection(n => n.EvidenceFileIds).HasField("_evidenceFileIds").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.PrimitiveCollection(n => n.Whys).HasField("_whys").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.PrimitiveCollection(n => n.CommitteeUserIds).HasField("_committeeUserIds").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.ComplexProperty(n => n.CostOfPoorQuality, m => m.ConfigureMoney());
            builder.ComplexProperty(n => n.ClosedBy, s => s.ConfigureSignature());

            builder.HasMany(n => n.Actions).WithOne().HasForeignKey("NonConformityId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(n => n.Actions).HasField("_actions").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class CorrectiveActionConfiguration : IEntityTypeConfiguration<CorrectiveAction>
    {
        public void Configure(EntityTypeBuilder<CorrectiveAction> builder)
        {
            builder.Property(a => a.ResponsibleName).HasMaxLength(NameLength);
            builder.HasIndex(a => a.ResponsibleUserId);
        }
    }

    internal class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.Property(a => a.Folio).HasMaxLength(CodeLength);
            builder.HasIndex(a => new { a.TenantId, a.Folio }).IsUnique();
            builder.HasIndex(a => a.PlannedStart);

            builder.HasMany(a => a.Participants).WithOne().HasForeignKey("AuditId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.Participants).HasField("_participants").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();

            builder.HasMany(a => a.Findings).WithOne().HasForeignKey("AuditId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.Findings).HasField("_findings").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class AuditParticipantConfiguration : IEntityTypeConfiguration<AuditParticipant>
    {
        public void Configure(EntityTypeBuilder<AuditParticipant> builder)
        {
            builder.Property(p => p.Name).HasMaxLength(NameLength);
            builder.Property(p => p.Email).HasMaxLength(NameLength);
        }
    }

    internal class AuditFindingConfiguration : IEntityTypeConfiguration<AuditFinding>
    {
        public void Configure(EntityTypeBuilder<AuditFinding> builder)
        {
            builder.Property(f => f.Clause).HasMaxLength(CodeLength);
            builder.HasOne<NonConformity>().WithMany().HasForeignKey(f => f.NonConformityId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    internal class ManagementReviewConfiguration : IEntityTypeConfiguration<ManagementReview>
    {
        public void Configure(EntityTypeBuilder<ManagementReview> builder)
        {
            builder.Property(r => r.Title).HasMaxLength(NameLength);
            builder.HasIndex(r => r.HeldOn);
            builder.PrimitiveCollection(r => r.Attendees).HasField("_attendees").UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.ComplexProperty(r => r.ApprovedBy, s => s.ConfigureSignature());

            builder.HasMany(r => r.Decisions).WithOne().HasForeignKey("ManagementReviewId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(r => r.Decisions).HasField("_decisions").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class ReviewDecisionConfiguration : IEntityTypeConfiguration<ReviewDecision>
    {
        public void Configure(EntityTypeBuilder<ReviewDecision> builder)
        {
            builder.Property(d => d.ResponsibleName).HasMaxLength(NameLength);
        }
    }

    internal class ComplianceRequirementConfiguration : IEntityTypeConfiguration<ComplianceRequirement>
    {
        public void Configure(EntityTypeBuilder<ComplianceRequirement> builder)
        {
            builder.Property(r => r.Edition).HasMaxLength(20);
            builder.Property(r => r.Clause).HasMaxLength(CodeLength);
            builder.Property(r => r.Title).HasMaxLength(NameLength);
            builder.HasIndex(r => new { r.Standard, r.Edition, r.Clause }).IsUnique();
        }
    }

    internal class RequirementAssessmentConfiguration : IEntityTypeConfiguration<RequirementAssessment>
    {
        public void Configure(EntityTypeBuilder<RequirementAssessment> builder)
        {
            builder.HasIndex(a => new { a.TenantId, a.RequirementId }).IsUnique();
            builder.HasOne<ComplianceRequirement>().WithMany().HasForeignKey(a => a.RequirementId).OnDelete(DeleteBehavior.Restrict);
            builder.PrimitiveCollection(a => a.EvidenceFileIds).HasField("_evidenceFileIds").UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }

    internal class QualityObjectiveConfiguration : IEntityTypeConfiguration<QualityObjective>
    {
        public void Configure(EntityTypeBuilder<QualityObjective> builder)
        {
            builder.Property(o => o.Name).HasMaxLength(NameLength);
            builder.Property(o => o.Indicator).HasMaxLength(NameLength);
            builder.Property(o => o.Unit).HasMaxLength(20);
            builder.Property(o => o.ResponsibleName).HasMaxLength(NameLength);
            builder.HasIndex(o => o.Year);

            builder.HasMany(o => o.Measurements).WithOne().HasForeignKey("QualityObjectiveId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(o => o.Measurements).HasField("_measurements").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class ObjectiveMeasurementConfiguration : IEntityTypeConfiguration<ObjectiveMeasurement>
    {
        public void Configure(EntityTypeBuilder<ObjectiveMeasurement> builder)
        {
            builder.HasIndex("QualityObjectiveId", nameof(ObjectiveMeasurement.Year), nameof(ObjectiveMeasurement.Month)).IsUnique();
        }
    }
}
