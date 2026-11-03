using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.ValueObjects;
using QMSteel.Domain.Welding;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Procedures
{
    public class WpsTests
    {
        [Fact]
        public void Approves_WhenPqrSupportsAllVariables()
        {
            var pqr = ApprovedPqr();
            var wps = ReviewedWps();
            wps.LinkPqr(pqr.Id);
            wps.SubmitForReview();

            var discrepancies = wps.Approve(Signature(), Today, new[] { pqr });

            Assert.Empty(discrepancies);
            Assert.Equal(WpsStatus.Approved, wps.Status);
            Assert.True(wps.CheckEligibility().IsEligible);
        }

        [Fact]
        public void StaysInReview_WhenPqrDoesNotSupportAmperage()
        {
            var pqr = ApprovedPqr(Variables(new NumericRange(115, 125)));
            var wps = ReviewedWps();
            wps.LinkPqr(pqr.Id);
            wps.SubmitForReview();

            var discrepancies = wps.Approve(Signature(), Today, new[] { pqr });

            var discrepancy = Assert.Single(discrepancies);
            Assert.StartsWith("Amperaje", discrepancy.Variable);
            Assert.Equal(WpsStatus.InReview, wps.Status);
            Assert.Equal("WPS_IN_REVIEW", wps.CheckEligibility().ReasonCode);
        }

        [Fact]
        public void DetectsFillerAndPreheatDiscrepancies()
        {
            var pqr = ApprovedPqr(Variables(new NumericRange(100, 140), filler: "E7018", preheat: 50));
            var wps = ReviewedWps(Variables(filler: "E6010", preheat: 10));
            wps.LinkPqr(pqr.Id);
            wps.SubmitForReview();

            var variables = wps.Approve(Signature(), Today, new[] { pqr }).Select(d => d.Variable).ToList();

            Assert.Contains("Material de aporte", variables);
            Assert.Contains("Precalentamiento mínimo (°C)", variables);
        }

        [Fact]
        public void IgnoresPqrsThatAreNotLinked()
        {
            var wps = ReviewedWps();
            wps.SubmitForReview();

            ThrowsDomain(() => wps.Approve(Signature(), Today, new[] { ApprovedPqr() }), "WPS_PQR_REQUIRED");
        }

        [Fact]
        public void IgnoresDraftPqrs()
        {
            var draftPqr = new Pqr(TenantId, "PQR-002", Variables(new NumericRange(100, 140)), Today);
            var wps = ReviewedWps();
            wps.LinkPqr(draftPqr.Id);
            wps.SubmitForReview();

            var discrepancy = Assert.Single(wps.Approve(Signature(), Today, new[] { draftPqr }));
            Assert.Equal("Proceso", discrepancy.Variable);
        }

        [Fact]
        public void Prequalified_ApprovesWithoutPqr()
        {
            var wps = ApprovedWps();

            Assert.Equal(WpsStatus.Approved, wps.Status);
        }

        [Fact]
        public void Submit_RequiresSignedFile()
        {
            var wps = new Wps(TenantId, "WPS-9", JointType.TJoint, WeldType.Fillet, Variables(), true);

            ThrowsDomain(wps.SubmitForReview, "WPS_FILE_REQUIRED");
        }

        [Fact]
        public void NewRevision_BlocksUseUntilReapproved()
        {
            var wps = ApprovedWps();

            wps.StartNewRevision();

            Assert.Equal(1, wps.Revision);
            Assert.Equal("WPS-001 Rev. 01", wps.Label);
            Assert.Equal(ResourceStatus.InReview, wps.GetStatus());
            Assert.False(wps.CheckEligibility().IsEligible);
        }

        [Fact]
        public void Obsolete_IsBlocked()
        {
            var wps = ApprovedWps();
            wps.MakeObsolete();

            Assert.Equal("WPS_OBSOLETE", wps.CheckEligibility().ReasonCode);
        }

        [Fact]
        public void ApprovedWps_IsNotEditable() =>
            ThrowsDomain(() => ApprovedWps().LinkPqr(Guid.NewGuid()), "WPS_NOT_EDITABLE");

        [Fact]
        public void Gmaw_RequiresShieldingGas() =>
            ThrowsDomain(() => new EssentialVariables(WeldingProcess.GMAW, "A36", "I", "ER70S-6", null, new[] { "1G" },
                new NumericRange(3, 10), 0, new NumericRange(150, 200), new NumericRange(20, 24), new NumericRange(200, 400)),
                "PROCEDURE_GAS_REQUIRED");

        [Fact]
        public void Pqr_RequiresFileToApprove()
        {
            var pqr = new Pqr(TenantId, "PQR-3", Variables(), Today);

            ThrowsDomain(() => pqr.Approve(Signature()), "PQR_FILE_REQUIRED");
        }
    }
}
