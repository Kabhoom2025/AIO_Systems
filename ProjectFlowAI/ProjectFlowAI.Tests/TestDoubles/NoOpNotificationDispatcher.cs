using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Tests.TestDoubles;

/// <summary>Does nothing — used by unit tests that construct a handler directly and don't care
/// about the notification side effect (it's covered separately by integration-style tests).</summary>
public class NoOpNotificationDispatcher : INotificationDispatcher
{
    public Task DispatchAsync(Guid userId, NotificationType type, string title, string body,
        string? linkUrl = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
