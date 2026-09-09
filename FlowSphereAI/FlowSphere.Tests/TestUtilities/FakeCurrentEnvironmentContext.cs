using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Tests.TestUtilities;

public class FakeCurrentEnvironmentContext : ICurrentEnvironmentContext
{
    public EnvironmentStage Stage { get; init; } = EnvironmentStage.Dev;
}
