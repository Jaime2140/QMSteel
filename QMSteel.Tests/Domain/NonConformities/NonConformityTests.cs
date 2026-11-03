using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.ValueObjects;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.NonConformities
{
    public class NonConformityTests
    {
        private static NonConformity NewNc(NonConformitySource source = NonConformitySource.Internal) =>
            new(TenantId, "nc-2026-0001", source, Criticality.High, DefectType.Welding, "Falta de fusión en junta J-1",
                Today, Guid.NewGuid(), Now, projectName: "Obra A");

        private static NonConformity ReadyToClose(bool requiresCorrective = true)
        {
            var nc = NewNc();
            nc.RecordRootCauseAnalysis(new[] { "Amperaje bajo", "Máquina descalibrada" }, "Falta de mantenimiento", requiresCorrective);
            var action = nc.AddAction(ActionType.Corrective, "Programa de mantenimiento", "Supervisor", Today.AddDays(10));
            nc.CompleteAction(action.Id, Today.AddDays(4));
            return nc;
        }

        [Fact]
        public void Opening_RaisesEvent()
        {
            var nc = NewNc();

            Assert.Equal("NC-2026-0001", nc.Folio);
            Assert.IsType<NonConformityOpened>(Assert.Single(nc.DomainEvents));
        }

        [Fact]
        public void Actions_RequireRootCauseAnalysis() =>
            ThrowsDomain(() => NewNc().AddAction(ActionType.Correction, "Reparar", "Soldador", Today), "NC_ANALYSIS_REQUIRED");

        [Fact]
        public void Analysis_AllowsAtMostFiveWhys() =>
            ThrowsDomain(() => NewNc().RecordRootCauseAnalysis(new[] { "1", "2", "3", "4", "5", "6" }, "Causa", false), "NC_TOO_MANY_WHYS");

        [Fact]
        public void CannotClose_WithPendingActions()
        {
            var nc = NewNc();
            nc.RecordRootCauseAnalysis(new[] { "Por qué" }, "Causa", false);
            nc.AddAction(ActionType.Correction, "Reparar", "Soldador", Today.AddDays(2));

            ThrowsDomain(() => nc.Close(Signature(), Today, true), "NC_NOT_READY_TO_CLOSE");
        }

        [Fact]
        public void EscalatedNc_RequiresCorrectiveAction()
        {
            var nc = NewNc();
            nc.RecordRootCauseAnalysis(new[] { "Por qué" }, "Causa", requiresCorrectiveAction: true);
            var action = nc.AddAction(ActionType.Correction, "Reparar", "Soldador", Today.AddDays(2));
            nc.CompleteAction(action.Id, Today);

            ThrowsDomain(() => nc.Close(Signature(), Today, true), "NC_CORRECTIVE_ACTION_REQUIRED");
        }

        [Fact]
        public void ProductNc_RequiresDisposition()
        {
            var nc = NewNc(NonConformitySource.Inspection);
            nc.RecordRootCauseAnalysis(new[] { "Por qué" }, "Causa", false);
            var action = nc.AddAction(ActionType.Correction, "Reparar", "Soldador", Today.AddDays(2));
            nc.CompleteAction(action.Id, Today);

            ThrowsDomain(() => nc.Close(Signature(), Today, true), "NC_DISPOSITION_REQUIRED");

            nc.SetDisposition(Disposition.Repair);
            nc.Close(Signature(), Today, true);
            Assert.Equal(NonConformityStatus.Closed, nc.Status);
        }

        [Fact]
        public void Close_RecordsEffectivenessAndDays()
        {
            var nc = ReadyToClose();

            nc.Close(Signature(), Today.AddDays(6), wasEffective: true);

            Assert.Equal(NonConformityStatus.Closed, nc.Status);
            Assert.Equal(6, nc.DaysToClose);
            Assert.Contains(nc.DomainEvents, e => e is NonConformityClosed { WasEffective: true });
            ThrowsDomain(() => nc.AddEvidence(Guid.NewGuid()), "NC_FINISHED");
        }

        [Fact]
        public void AddingActionAfterAllCompleted_ReopensActions()
        {
            var nc = ReadyToClose();

            nc.AddAction(ActionType.Preventive, "Capacitar", "Calidad", Today.AddDays(20));

            Assert.Equal(NonConformityStatus.ActionsInProgress, nc.Status);
        }

        [Fact]
        public void Overdue_WhenActionPastDue()
        {
            var nc = NewNc();
            nc.RecordRootCauseAnalysis(new[] { "Por qué" }, "Causa", false);
            nc.AddAction(ActionType.Correction, "Reparar", "Soldador", Today);

            Assert.False(nc.IsOverdueOn(Today));
            Assert.True(nc.IsOverdueOn(Today.AddDays(1)));
        }

        [Fact]
        public void CostOfPoorQuality_Accumulates()
        {
            var nc = NewNc();
            nc.RegisterCost(new Money(1000, Currency.USD));
            nc.RegisterCost(new Money(250.50m, Currency.USD));

            Assert.Equal(1250.50m, nc.CostOfPoorQuality!.Amount);
            ThrowsDomain(() => nc.RegisterCost(new Money(10, Currency.MXN)), "MONEY_CURRENCY_MISMATCH");
        }

        [Fact]
        public void Kpis_ClosureEffectivenessAndAverageDays()
        {
            var effective = ReadyToClose();
            effective.Close(Signature(), Today.AddDays(4), true);
            var ineffective = ReadyToClose();
            ineffective.Close(Signature(), Today.AddDays(10), false);
            var open = NewNc();

            var all = new[] { effective, ineffective, open };

            Assert.Equal(50m, QualityKpis.ClosureEffectivenessPercentage(all));
            Assert.Equal(7m, QualityKpis.AverageDaysToClose(all));
        }
    }
}
