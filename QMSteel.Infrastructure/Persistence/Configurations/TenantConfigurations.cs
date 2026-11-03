using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Entities.Users;
using static QMSteel.Infrastructure.Persistence.Configurations.MappingExtensions;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.Property(t => t.Name).HasMaxLength(NameLength);
            builder.Property(t => t.Slug).HasMaxLength(100);
            builder.HasIndex(t => t.Slug).IsUnique();
            builder.PrimitiveCollection(t => t.EnabledStandards)
                .HasField("_enabledStandards")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .ElementType(e => e.HasConversion<string>());
        }
    }

    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.IdentityId).HasMaxLength(NameLength);
            builder.Property(u => u.Email).HasMaxLength(NameLength);
            builder.Property(u => u.FullName).HasMaxLength(NameLength);
            builder.HasIndex(u => u.IdentityId).IsUnique();
            builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        }
    }

    internal class FolioSequenceConfiguration : IEntityTypeConfiguration<FolioSequence>
    {
        public void Configure(EntityTypeBuilder<FolioSequence> builder)
        {
            builder.HasKey(f => new { f.TenantId, f.Prefix, f.Year });
            builder.Property(f => f.Prefix).HasMaxLength(20);
        }
    }
}
