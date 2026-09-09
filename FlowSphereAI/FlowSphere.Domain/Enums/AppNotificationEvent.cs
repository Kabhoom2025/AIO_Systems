namespace FlowSphere.Domain.Enums;

/// <summary>RecordSubmitted and Custom actually dispatch (hooked into SubmitAppCommandHandler /
/// SubmitGuestAppRecordCommandHandler). TaskCompleted/TaskCompletedInitiator are configurable in
/// the rule builder but do not fire yet - they need a workflow step-completion hook that doesn't
/// exist today; the client flags them "not yet wired" rather than silently pretending they work.</summary>
public enum AppNotificationEvent
{
    RecordSubmitted,
    Custom,
    TaskCompleted,
    TaskCompletedInitiator
}
