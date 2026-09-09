using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Billing.Commands.ChangePlan;

public record ChangePlanCommand(string PlanName) : IRequest<Result>;
