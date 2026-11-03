using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Dashboard
{
    // Tablero de alertas: todo lo que vence dentro del horizonte indicado o que ya venció.
    public sealed class AlertService : ApplicationService
    {
        public const int DefaultHorizonDays = 60;
        public const int ExpiringSoonDays = 30;

        private readonly IControlledDocumentRepository _documents;
        private readonly IEquipmentRepository _equipments;
        private readonly IWelderRepository _welders;
        private readonly IInspectorRepository _inspectors;

        public AlertService(UseCaseContext context, IControlledDocumentRepository documents, IEquipmentRepository equipments,
            IWelderRepository welders, IInspectorRepository inspectors) : base(context)
        {
            _documents = documents;
            _equipments = equipments;
            _welders = welders;
            _inspectors = inspectors;
        }

        public async Task<IReadOnlyList<ExpiryAlertDto>> GetExpiryAlertsAsync(int horizonDays = DefaultHorizonDays, CancellationToken ct = default)
        {
            var alerts = new List<ExpiryAlertDto>();

            foreach (var document in await _documents.GetAllAsync(true, ct))
                if (document.CurrentRevision?.NextReviewOn is { } reviewOn)
                    alerts.Add(Alert(AlertModule.Documents, document.Id, $"{document.Code} {document.Title}",
                        $"Revisión periódica {document.CurrentRevision.Label}", reviewOn));

            foreach (var equipment in await _equipments.GetAllAsync(true, ct))
                if (equipment.LastCalibration is { } calibration)
                    alerts.Add(Alert(AlertModule.Equipments, equipment.Id, $"{equipment.Code} {equipment.Name}", "Calibración", calibration.DueOn));

            foreach (var welder in await _welders.GetAllAsync(true, ct))
                foreach (var qualification in welder.Qualifications.Where(q => !q.IsRevoked))
                    alerts.Add(Alert(AlertModule.Welders, welder.Id, $"{welder.StampNumber} {welder.FullName}",
                        $"Calificación {qualification.Process} {qualification.TestPosition}", qualification.EffectiveExpiry));

            foreach (var inspector in await _inspectors.GetAllAsync(true, ct))
            {
                if (inspector.LastVisionExam is { } exam)
                    alerts.Add(Alert(AlertModule.Inspectors, inspector.Id, inspector.FullName, "Examen de la vista", exam.ExpiresOn));

                var latestPerCertification = inspector.Certifications.GroupBy(c => (c.Scheme, c.Method)).Select(g => g.MaxBy(c => c.ExpiresOn)!);
                foreach (var certification in latestPerCertification)
                    alerts.Add(Alert(AlertModule.Inspectors, inspector.Id, inspector.FullName, certification.Describe(), certification.ExpiresOn));
            }

            return alerts.Where(a => a.DaysRemaining <= horizonDays).OrderBy(a => a.DaysRemaining).ToList();
        }

        private ExpiryAlertDto Alert(AlertModule module, Guid resourceId, string resourceName, string item, DateOnly expiresOn) =>
            new(module, resourceId, resourceName, item, expiresOn, ExpiryAlerts.DaysUntil(expiresOn, Today),
                ExpiryAlerts.StatusFor(expiresOn, Today, ExpiringSoonDays));
    }
}
