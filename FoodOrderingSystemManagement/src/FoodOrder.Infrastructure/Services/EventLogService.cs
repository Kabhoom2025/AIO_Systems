using FoodOrder.Application.DTOs.SystemHealth;
using System.Collections.Concurrent;

namespace FoodOrder.Infrastructure.Services;

/// <summary>
/// Singleton in-memory ring buffer for system health events (last 100 entries).
/// </summary>
public class EventLogService
{
    private readonly ConcurrentQueue<HealthEventDto> _events = new();
    private const int MaxEvents = 100;

    public void Log(string level, string message, string category = "System")
    {
        _events.Enqueue(new HealthEventDto
        {
            Timestamp = DateTime.UtcNow,
            Level     = level,
            Category  = category,
            Message   = message,
        });
        // Trim oldest entries when over capacity
        while (_events.Count > MaxEvents)
            _events.TryDequeue(out _);
    }

    public IReadOnlyList<HealthEventDto> GetRecent(int count = 20)
        => [.. _events.Reverse().Take(count)];
}
