using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Infrastructure.Data;

namespace ProjectFlowAI.Tests.TestDoubles;

/// <summary>Each call creates a fresh, isolated EF Core InMemory-backed ProjectFlowDbContext —
/// used as the IProjectFlowDbContext fake for handler unit tests (see spec: "in-memory fakes ...
/// for IProjectFlowDbContext").</summary>
public static class InMemoryDbContextFactory
{
    public static ProjectFlowDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ProjectFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ProjectFlowDbContext(options);
    }
}
