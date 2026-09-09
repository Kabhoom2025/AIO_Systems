using FlowSphere.Application.Common;
using Xunit;

namespace FlowSphere.Tests.Application.Common;

public class WorkflowGraphNodeConfigTests
{
    [Fact]
    public void GetAssigneeLabel_NodeHasLabel_ReturnsIt()
    {
        const string graph = """{"nodes":[{"key":"review","type":"UserTask","config":{"assigneeLabel":"Finance Team"}}],"edges":[]}""";

        Assert.Equal("Finance Team", WorkflowGraphNodeConfig.GetAssigneeLabel(graph, "review"));
    }

    [Fact]
    public void GetAssigneeLabel_NodeHasNoLabel_ReturnsNull()
    {
        const string graph = """{"nodes":[{"key":"review","type":"UserTask","config":{}}],"edges":[]}""";

        Assert.Null(WorkflowGraphNodeConfig.GetAssigneeLabel(graph, "review"));
    }

    [Fact]
    public void GetAssigneeLabel_NodeKeyNull_ReturnsNull()
    {
        const string graph = """{"nodes":[{"key":"review","type":"UserTask","config":{"assigneeLabel":"HR"}}],"edges":[]}""";

        Assert.Null(WorkflowGraphNodeConfig.GetAssigneeLabel(graph, null));
    }

    [Fact]
    public void GetAssigneeLabel_NodeNotFound_ReturnsNull()
    {
        const string graph = """{"nodes":[{"key":"other","type":"UserTask","config":{"assigneeLabel":"HR"}}],"edges":[]}""";

        Assert.Null(WorkflowGraphNodeConfig.GetAssigneeLabel(graph, "review"));
    }

    [Fact]
    public void GetAssigneeLabel_InvalidJson_ReturnsNull()
    {
        Assert.Null(WorkflowGraphNodeConfig.GetAssigneeLabel("not json", "review"));
    }
}
