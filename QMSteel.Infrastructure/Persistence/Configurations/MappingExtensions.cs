using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Infrastructure.Persistence.Configurations
{
    internal static class MappingExtensions
    {
        public const int CodeLength = 50;
        public const int NameLength = 200;

        public static void ConfigureSignature(this ComplexPropertyBuilder<ElectronicSignature> signature)
        {
            signature.Property(s => s.SignerName).HasMaxLength(NameLength);
            signature.Property(s => s.Meaning).HasMaxLength(100);
        }

        public static void ConfigureMoney(this ComplexPropertyBuilder<Money> money)
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
        }

        // Las variables esenciales se guardan como un documento JSON: se leen y validan siempre completas.
        public static void MapVariables<TOwner>(OwnedNavigationBuilder<TOwner, EssentialVariables> variables) where TOwner : class
        {
            variables.ToJson();
            variables.OwnsOne(v => v.ThicknessMm);
            variables.OwnsOne(v => v.Amperage);
            variables.OwnsOne(v => v.Voltage);
            variables.OwnsOne(v => v.TravelSpeedMmPerMin);
        }
    }
}
