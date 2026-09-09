using FlowSphere.Domain.Enums;

namespace FlowSphere.Application.Common;

public interface ICurrentEnvironmentContext
{
    EnvironmentStage Stage { get; }
}
