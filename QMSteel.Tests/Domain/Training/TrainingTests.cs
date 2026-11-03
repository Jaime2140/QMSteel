using QMSteel.Domain.Entities.Training;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Training
{
    public class TrainingTests
    {
        private static TrainingSession HeldSession()
        {
            var session = new TrainingSession(TenantId, "Inspección visual AWS D1.1", "Instructor", Today, 8, passingScore: 80);
            session.AddAttendee("E-1", "Persona Uno");
            session.AddAttendee("E-2", "Persona Dos");
            session.AddAttendee("E-3", "Persona Tres");
            session.RecordAttendance("E-1", true, 90);
            session.RecordAttendance("E-2", true, 70);
            session.RecordAttendance("E-3", false);
            session.MarkHeld(Guid.NewGuid());
            return session;
        }

        [Fact]
        public void Plan_CompliancePercentageExcludesCancelled()
        {
            var plan = new TrainingPlan(TenantId, 2026);
            var done = plan.AddCourse("Seguridad", 2, true);
            plan.AddCourse("Primeros auxilios", 5, false);
            var cancelled = plan.AddCourse("Excel", 6, false);

            plan.CompleteCourse(done.Id, Guid.NewGuid());
            plan.CancelCourse(cancelled.Id, "Sin presupuesto");

            Assert.Equal(50m, plan.CompliancePercentage());
        }

        [Fact]
        public void Plan_ListsOverdueCourses()
        {
            var plan = new TrainingPlan(TenantId, 2026);
            plan.AddCourse("Atrasado", 10, true);
            plan.AddCourse("A tiempo", 11, true);

            Assert.Equal("Atrasado", Assert.Single(plan.OverdueCourses(Today)).Title);
        }

        [Fact]
        public void Plan_RejectsInvalidMonth() =>
            ThrowsDomain(() => new TrainingPlan(TenantId, 2026).AddCourse("Curso", 13, false), "TRAINING_COURSE_MONTH_INVALID");

        [Fact]
        public void MarkHeld_RequiresAttendanceOfEveryone()
        {
            var session = new TrainingSession(TenantId, "Curso", "Instructor", Today, 4);
            session.AddAttendee("E-1", "Persona");

            ThrowsDomain(() => session.MarkHeld(Guid.NewGuid()), "TRAINING_ATTENDANCE_PENDING");
        }

        [Fact]
        public void Certificate_OnlyForApprovedAttendees()
        {
            var session = HeldSession();

            session.IssueCertificate("E-1", "DC3-0001");

            Assert.Equal("DC3-0001", session.Attendees.Single(a => a.EmployeeNumber == "E-1").CertificateFolio);
            ThrowsDomain(() => session.IssueCertificate("E-2", "DC3-0002"), "TRAINING_NOT_APPROVED");
            ThrowsDomain(() => session.IssueCertificate("E-3", "DC3-0003"), "TRAINING_NOT_APPROVED");
        }

        [Fact]
        public void Close_RequiresEffectivenessOfAttendees()
        {
            var session = HeldSession();
            session.EvaluateEffectiveness("E-1", true, Today.AddMonths(1));

            ThrowsDomain(session.Close, "TRAINING_EFFECTIVENESS_PENDING");

            session.EvaluateEffectiveness("E-2", false, Today.AddMonths(1));
            session.Close();

            Assert.Equal(TrainingSessionStatus.Closed, session.Status);
            Assert.Equal(50m, session.EffectivenessPercentage());
        }

        [Fact]
        public void Effectiveness_OnlyForAttendees() =>
            ThrowsDomain(() => HeldSession().EvaluateEffectiveness("E-3", true, Today), "TRAINING_NOT_ATTENDED");

        [Fact]
        public void DuplicateAttendee_IsRejected()
        {
            var session = new TrainingSession(TenantId, "Curso", "Instructor", Today, 4);
            session.AddAttendee("E-1", "Persona");

            ThrowsDomain(() => session.AddAttendee(" E-1 ", "Persona"), "TRAINING_ATTENDEE_DUPLICATED");
        }
    }
}
