using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;

namespace FlowSphere.Application.Copilot.Commands.GenerateApp;

public class GenerateAppCommandHandler : IRequestHandler<GenerateAppCommand, Result<GeneratedAppDto>>
{
    private readonly IAppFormGenerator _generator;
    private readonly ICurrentUserContext _currentUser;

    public GenerateAppCommandHandler(IAppFormGenerator generator, ICurrentUserContext currentUser)
    {
        _generator = generator;
        _currentUser = currentUser;
    }

    public async Task<Result<GeneratedAppDto>> Handle(GenerateAppCommand request, CancellationToken cancellationToken)
    {
        var result = await _generator.GenerateAsync(request.Prompt, _currentUser.OrganizationId, cancellationToken);

        return result.IsSuccess
            ? Result<GeneratedAppDto>.Success(new GeneratedAppDto(result.Value!.Name, result.Value.Description, result.Value.FormSchemaJson))
            : Result<GeneratedAppDto>.Failure(result.Error!);
    }
}
