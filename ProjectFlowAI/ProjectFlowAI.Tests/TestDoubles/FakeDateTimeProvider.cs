using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Tests.TestDoubles;

/// <summary>Controllable clock for deterministic timer-elapsed-minutes assertions
/// (StartTimer/StopTimer tests need to advance time without a real Task.Delay).</summary>
public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}
