using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Connectors.Queries.GetConnectorsList;

public record GetConnectorsListQuery : IRequest<Result<List<ConnectorSummaryDto>>>;

public record ConnectorSummaryDto(
    int Id, string Type, string Name, bool IsEnabled, string[] RequiredCredentialKeys, string[] OptionalCredentialKeys);
