namespace FlowSphere.Domain.Common;

public record PlanDefinition(string Name, int MonthlyExecutionQuota);

/// <summary>Static plan tiers - no payment processor is wired up yet, so "upgrading" just
/// changes the organization's quota directly. A real billing integration (Stripe or similar)
/// would sit in front of this and call the same underlying change once payment succeeds.</summary>
public static class PlanCatalog
{
    public static readonly PlanDefinition Free = new("Free", 100);
    public static readonly PlanDefinition Pro = new("Pro", 1000);
    public static readonly PlanDefinition Enterprise = new("Enterprise", 10000);

    public static readonly IReadOnlyList<PlanDefinition> All = new[] { Free, Pro, Enterprise };

    public static PlanDefinition? Find(string name) => All.FirstOrDefault(p => p.Name == name);
}
