using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Training;
using QMSteel.Domain.Entities.Users;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class InspectorConfiguration : IEntityTypeConfiguration<Inspector>
    {
        public void Configure(EntityTypeBuilder<Inspector> builder)
        {
            builder.Property(i => i.EmployeeNumber).HasMaxLength(CodeLength);
            builder.Property(i => i.FullName).HasMaxLength(NameLength);
            builder.HasIndex(i => new { i.TenantId, i.EmployeeNumber }).IsUnique();
            builder.HasOne<User>().WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Certifications).WithOne().HasForeignKey("InspectorId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.Certifications).HasField("_certifications").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();

            builder.HasMany(i => i.VisionExams).WithOne().HasForeignKey("InspectorId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(i => i.VisionExams).HasField("_visionExams").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class InspectorCertificationConfiguration : IEntityTypeConfiguration<InspectorCertification>
    {
        public void Configure(EntityTypeBuilder<InspectorCertification> builder)
        {
            builder.Property(c => c.CertificateNumber).HasMaxLength(CodeLength);
            builder.Property(c => c.IssuingBody).HasMaxLength(NameLength);
        }
    }

    internal class TrainingPlanConfiguration : IEntityTypeConfiguration<TrainingPlan>
    {
        public void Configure(EntityTypeBuilder<TrainingPlan> builder)
        {
            builder.HasIndex(p => new { p.TenantId, p.Year }).IsUnique();
            builder.HasMany(p => p.Courses).WithOne().HasForeignKey("TrainingPlanId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(p => p.Courses).HasField("_courses").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class TrainingSessionConfiguration : IEntityTypeConfiguration<TrainingSession>
    {
        public void Configure(EntityTypeBuilder<TrainingSession> builder)
        {
            builder.Property(s => s.CourseTitle).HasMaxLength(NameLength);
            builder.Property(s => s.Instructor).HasMaxLength(NameLength);
            builder.HasIndex(s => s.PlannedCourseId);
            builder.HasMany(s => s.Attendees).WithOne().HasForeignKey("TrainingSessionId").OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(s => s.Attendees).HasField("_attendees").UsePropertyAccessMode(PropertyAccessMode.Field).AutoInclude();
        }
    }

    internal class TrainingAttendeeConfiguration : IEntityTypeConfiguration<TrainingAttendee>
    {
        public void Configure(EntityTypeBuilder<TrainingAttendee> builder)
        {
            builder.Property(a => a.EmployeeNumber).HasMaxLength(CodeLength);
            builder.Property(a => a.FullName).HasMaxLength(NameLength);
            builder.Property(a => a.CertificateFolio).HasMaxLength(CodeLength);
            builder.HasIndex(a => a.EmployeeNumber);
        }
    }
}
