namespace FlowSphere.Domain.Enums;

public enum AppTriggerActionType
{
    /// <summary>Creates and submits a record in the destination app automatically, with no
    /// workflow execution started even if the destination app has a linked workflow.</summary>
    SubmitAddRecord,

    /// <summary>Creates a record in the destination app and, if it has a linked workflow, also
    /// starts a real WorkflowExecution from it - so an approval/UserTask step genuinely appears
    /// for whoever that workflow routes it to.</summary>
    CreateTask
}
