using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Suppliers
{
    public class SupplierAndReceivingTests
    {
        private static Supplier ApprovedSupplier()
        {
            var supplier = new Supplier(TenantId, "Aceros del Norte", new[] { SupplierCategory.Steel });
            supplier.Evaluate(EvaluationKind.Initial, Today, 90, Signature("Evaluó"));
            return supplier;
        }

        private static ReceivingInspection NewReceiving() =>
            new(TenantId, "rec-001", ApprovedSupplier(), "OC-100", "REM-55", Today);

        [Theory]
        [InlineData(85, SupplierStatus.Approved)]
        [InlineData(70, SupplierStatus.Conditional)]
        [InlineData(40, SupplierStatus.Prospect)]
        public void InitialEvaluation_SetsAvlStatus(int score, SupplierStatus expected)
        {
            var supplier = new Supplier(TenantId, "Proveedor", new[] { SupplierCategory.Coatings });

            supplier.Evaluate(EvaluationKind.Initial, Today, score, Signature());

            Assert.Equal(expected, supplier.Status);
            Assert.Equal((decimal)score, supplier.Score);
        }

        [Fact]
        public void LowPeriodicScore_SuspendsSupplier()
        {
            var supplier = ApprovedSupplier();

            supplier.Evaluate(EvaluationKind.Periodic, Today.AddMonths(6), 50, Signature());

            Assert.Equal(SupplierStatus.Suspended, supplier.Status);
            Assert.Equal("SUPPLIER_SUSPENDED", supplier.CheckEligibility().ReasonCode);
        }

        [Fact]
        public void OnlyOneInitialEvaluation() =>
            ThrowsDomain(() => ApprovedSupplier().Evaluate(EvaluationKind.Initial, Today, 95, Signature()), "SUPPLIER_INITIAL_EVALUATED");

        [Fact]
        public void Receiving_BlockedForSupplierOutsideAvl()
        {
            var prospect = new Supplier(TenantId, "Nuevo", new[] { SupplierCategory.Steel });

            ThrowsDomain(() => new ReceivingInspection(TenantId, "REC-1", prospect, "OC-1", "REM-1", Today), "SUPPLIER_NOT_APPROVED");
        }

        [Fact]
        public void Steel_RequiresHeatNumber() =>
            ThrowsDomain(() => NewReceiving().AddItem(MaterialType.Steel, "Viga W12x26", 10, "pza"), "RECEIVING_HEAT_REQUIRED");

        [Fact]
        public void Steel_WithoutMtrDefaultsToMissing()
        {
            var item = NewReceiving().AddItem(MaterialType.Steel, "Viga W12x26", 10, "pza", "ASTM A992", "h-123");

            Assert.Equal(MtrStatus.Missing, item.MtrStatus);
            Assert.Equal("H-123", item.HeatNumber);
        }

        [Fact]
        public void Steel_CannotBeMarkedMtrNotRequired() =>
            ThrowsDomain(() => NewReceiving().AddItem(MaterialType.Steel, "Placa", 1, "pza", heatNumber: "H-1",
                mtrStatus: MtrStatus.NotRequired), "RECEIVING_MTR_REQUIRED");

        [Fact]
        public void ConformingReceipt_IsAccepted()
        {
            var receiving = NewReceiving();
            var item = receiving.AddItem(MaterialType.Steel, "Placa 1/2", 5, "pza", "ASTM A36", "H-1", MtrStatus.Complete, Guid.NewGuid());
            receiving.RecordVisualInspection(item.Id, accepted: true);

            receiving.Complete(Signature("Inspeccionó"));

            Assert.Equal(ReceivingStatus.Accepted, receiving.Status);
            Assert.Empty(receiving.DomainEvents);
        }

        [Fact]
        public void IncompleteMtrOrDamage_RejectsAndRaisesNcr()
        {
            var receiving = NewReceiving();
            var steel = receiving.AddItem(MaterialType.Steel, "Placa 1/2", 5, "pza", "ASTM A36", "H-1", MtrStatus.Incomplete, Guid.NewGuid());
            var paint = receiving.AddItem(MaterialType.Coating, "Primario epóxico", 4, "cubeta");
            receiving.RecordVisualInspection(steel.Id, accepted: true);
            receiving.RecordVisualInspection(paint.Id, accepted: false, new[] { ReceivingDefect.PhysicalDamage });

            receiving.Complete(Signature("Inspeccionó"));

            Assert.Equal(ReceivingStatus.Rejected, receiving.Status);
            var ncr = Assert.IsType<ReceivingNonConformityDetected>(Assert.Single(receiving.DomainEvents));
            Assert.Equal(2, ncr.Deviations.Count);
            Assert.Contains(ncr.Deviations, d => d.Contains("MTR incompleto") && d.Contains("colada H-1"));
        }

        [Fact]
        public void Complete_RequiresVisualInspectionOfAllItems()
        {
            var receiving = NewReceiving();
            receiving.AddItem(MaterialType.Coating, "Primario", 4, "cubeta");

            ThrowsDomain(() => receiving.Complete(Signature()), "RECEIVING_VISUAL_PENDING");
        }
    }
}
