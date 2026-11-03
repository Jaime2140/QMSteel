namespace QMSteel.Domain.Entities.NonConformities
{
    public enum NonConformitySource { Inspection, Receiving, Audit, Customer, Internal }

    public enum Criticality { Low, Medium, High }

    public enum DefectType { Procedure, Dimensional, Welding, Material, Coating, Documentation, Other }

    public enum Disposition { UseAsIs, Rework, Repair, Scrap, ReturnToSupplier }

    public enum NonConformityStatus { Open, UnderAnalysis, ActionsInProgress, PendingClosure, Closed, Cancelled }

    public enum ActionType { Correction, Corrective, Preventive }
}
