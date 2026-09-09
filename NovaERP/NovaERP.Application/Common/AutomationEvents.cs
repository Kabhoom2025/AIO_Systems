namespace NovaERP.Application.Common;

/// <summary>Fixed vocabulary of trigger events an AutomationRule can react to. Callers (e.g.
/// IWorkflowInstanceService) must reference these constants rather than magic strings, and
/// AutomationRule validators check TriggerEvent against <see cref="All"/>.</summary>
public static class AutomationEvents
{
    public const string WorkflowApproved = "WorkflowInstance.Approved";
    public const string WorkflowRejected = "WorkflowInstance.Rejected";
    public const string DocumentUploaded = "Document.Uploaded";
    public const string LeadCreated = "Lead.Created";
    public const string LeadConverted = "Lead.Converted";
    public const string OpportunityWon = "Opportunity.Won";
    public const string OpportunityLost = "Opportunity.Lost";
    public const string SalesOrderConfirmed = "SalesOrder.Confirmed";
    public const string SalesOrderCancelled = "SalesOrder.Cancelled";
    public const string RfqSent = "Rfq.Sent";
    public const string RfqClosed = "Rfq.Closed";
    public const string PurchaseOrderConfirmed = "PurchaseOrder.Confirmed";
    public const string PurchaseOrderReceived = "PurchaseOrder.Received";
    public const string PurchaseOrderCancelled = "PurchaseOrder.Cancelled";

    public static readonly IReadOnlyList<string> All = new[]
    {
        WorkflowApproved, WorkflowRejected, DocumentUploaded,
        LeadCreated, LeadConverted, OpportunityWon, OpportunityLost,
        SalesOrderConfirmed, SalesOrderCancelled,
        RfqSent, RfqClosed,
        PurchaseOrderConfirmed, PurchaseOrderReceived, PurchaseOrderCancelled
    };
}
