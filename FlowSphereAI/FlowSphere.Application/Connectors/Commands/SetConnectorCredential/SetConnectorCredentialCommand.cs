using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Connectors.Commands.SetConnectorCredential;

public record SetConnectorCredentialCommand(int ConnectorId, string KeyName, string Value) : IRequest<Result>;
