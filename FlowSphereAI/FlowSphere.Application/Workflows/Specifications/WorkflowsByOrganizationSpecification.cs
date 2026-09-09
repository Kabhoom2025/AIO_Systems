using FlowSphere.Application.Specifications;
using FlowSphere.Domain.Entities;

namespace FlowSphere.Application.Workflows.Specifications;

public class WorkflowsByOrganizationSpecification : Specification<WorkflowDefinition>
{
    public WorkflowsByOrganizationSpecification(int organizationId, int page, int pageSize)
    {
        Criteria = w => w.OrganizationId == organizationId;
        OrderByDescending = w => w.CreatedDate;
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
