using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using FlowSphere.Domain.Enums;
using Xunit;

namespace FlowSphere.Tests.Application.Workflows;

public class WorkflowDefinitionPublishTests
{
    [Fact]
    public void Publish_DemotesPreviouslyPublishedVersion_ToArchived()
    {
        var workflow = WorkflowDefinition.Create(organizationId: 1, name: "Test", description: null, createdByUserId: 1);
        var v1 = workflow.CreateNextVersion("{}");
        v1.Id = 1; // EF Core would assign these on save; set explicitly here since this is a pure-domain test with no DbContext.
        var v2 = workflow.CreateNextVersion("{}");
        v2.Id = 2;

        workflow.Publish(v1.Id);
        Assert.Equal(VersionStatus.Published, v1.Status);

        workflow.Publish(v2.Id);

        Assert.Equal(VersionStatus.Archived, v1.Status);
        Assert.Equal(VersionStatus.Published, v2.Status);
        Assert.Single(workflow.Versions, v => v.Status == VersionStatus.Published);
    }

    [Fact]
    public void Publish_UnknownVersionId_ThrowsDomainException()
    {
        var workflow = WorkflowDefinition.Create(1, "Test", null, 1);
        workflow.CreateNextVersion("{}");

        Assert.Throws<DomainException>(() => workflow.Publish(versionId: 999));
    }

    [Fact]
    public void CreateNextVersion_IncrementsVersionNumber_Monotonically()
    {
        var workflow = WorkflowDefinition.Create(1, "Test", null, 1);

        var v1 = workflow.CreateNextVersion("{}");
        var v2 = workflow.CreateNextVersion("{}");
        var v3 = workflow.CreateNextVersion("{}");

        Assert.Equal(1, v1.VersionNumber);
        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal(3, v3.VersionNumber);
    }
}
