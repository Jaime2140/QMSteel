using Microsoft.EntityFrameworkCore;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Entities.Welders;

namespace QMSteel.Tests.Infrastructure
{
    public class ModelMappingTests
    {
        private static readonly Lazy<string> CreateScript = new(() => TestDbContextFactory.Create().Database.GenerateCreateScript());

        [Fact]
        public void Model_GeneratesCreateScript()
        {
            Assert.Contains("CREATE TABLE", CreateScript.Value);
        }

        // Si una colección privada no se mapea, EF la ignora sin avisar y se pierden datos: se valida columna por columna.
        [Theory]
        [InlineData("\"QualifiedPositions\" text[]")]
        [InlineData("\"StandardClauses\" text[]")]
        [InlineData("\"SupportingPqrIds\" uuid[]")]
        [InlineData("\"Whys\" text[]")]
        [InlineData("\"CommitteeUserIds\" uuid[]")]
        [InlineData("\"EvidenceFileIds\" uuid[]")]
        [InlineData("\"EquipmentIds\" uuid[]")]
        [InlineData("\"PhotoFileIds\" uuid[]")]
        [InlineData("\"Readings\" numeric[]")]
        [InlineData("\"Attendees\" text[]")]
        [InlineData("\"Categories\" text[]")]
        [InlineData("\"Defects\" text[]")]
        [InlineData("\"EnabledStandards\" text[]")]
        [InlineData("\"Variables\" jsonb")]
        [InlineData("\"PreparedBy_SignerName\"")]
        [InlineData("\"ReviewedBy_UserId\" uuid")]
        [InlineData("\"CostOfPoorQuality_Amount\" numeric(18,2)")]
        [InlineData("\"CostOfPoorQuality_Currency\" character varying(40)")]
        [InlineData("\"Status\" character varying(40)")]
        public void Script_MapsColumn(string columnDefinition)
        {
            Assert.Contains(columnDefinition, CreateScript.Value);
        }

        [Fact]
        public void AggregateRoots_IgnoreDomainEventsAndUseConcurrencyToken()
        {
            using var context = TestDbContextFactory.Create();

            foreach (var entityType in context.Model.GetEntityTypes().Where(t => typeof(AggregateRoot).IsAssignableFrom(t.ClrType)))
            {
                Assert.Null(entityType.FindNavigation(nameof(AggregateRoot.DomainEvents)));
                Assert.True(entityType.FindProperty("xmin")?.IsConcurrencyToken, $"{entityType.ClrType.Name} sin token de concurrencia.");
            }
        }

        [Fact]
        public void TenantEntities_HaveQueryFilter()
        {
            using var context = TestDbContextFactory.Create();

            var withoutFilter = context.Model.GetEntityTypes()
                .Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType) && t.GetDeclaredQueryFilters().Count == 0)
                .Select(t => t.ClrType.Name)
                .ToList();

            Assert.Empty(withoutFilter);
        }

        [Fact]
        public void Query_FiltersByCurrentTenant()
        {
            var user = new FakeCurrentUser();
            using var context = TestDbContextFactory.Create(user);

            var sql = context.Welders.Where(w => w.IsActive).ToQueryString();

            Assert.Contains("\"TenantId\" = @", sql);
            Assert.Contains("JOIN \"WelderQualifications\"", sql);
        }

        [Fact]
        public void ChildEntities_UseClientGeneratedKeys()
        {
            using var context = TestDbContextFactory.Create();

            var key = context.Model.FindEntityType(typeof(WelderQualification))!.FindPrimaryKey()!.Properties.Single();

            Assert.Equal(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never, key.ValueGenerated);
        }
    }
}
