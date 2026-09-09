using FlowSphere.Application.Common;
using FlowSphere.Application.Copilot.Commands.GenerateApp;
using FlowSphere.Application.Interfaces;
using FlowSphere.Tests.TestUtilities;
using Xunit;

namespace FlowSphere.Tests.Application.Copilot;

public class GenerateAppCommandHandlerTests
{
    private class FakeAppFormGenerator : IAppFormGenerator
    {
        public Result<GeneratedAppFormResult>? Response { get; set; }
        public string? ReceivedPrompt { get; private set; }

        public Task<Result<GeneratedAppFormResult>> GenerateAsync(string prompt, int organizationId, CancellationToken cancellationToken)
        {
            ReceivedPrompt = prompt;
            return Task.FromResult(Response!);
        }
    }

    [Fact]
    public async Task Handle_GeneratorSucceeds_ReturnsGeneratedAppDto()
    {
        var generator = new FakeAppFormGenerator
        {
            Response = Result<GeneratedAppFormResult>.Success(
                new GeneratedAppFormResult("Leave Request", "Collects leave requests", """{"sections":[]}""")),
        };
        var handler = new GenerateAppCommandHandler(generator, new FakeCurrentUserContext());

        var result = await handler.Handle(new GenerateAppCommand("A leave request form"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Leave Request", result.Value!.Name);
        Assert.Equal("Collects leave requests", result.Value.Description);
        Assert.Equal("""{"sections":[]}""", result.Value.FormSchemaJson);
        Assert.Equal("A leave request form", generator.ReceivedPrompt);
    }

    [Fact]
    public async Task Handle_GeneratorFails_PropagatesFailure()
    {
        var generator = new FakeAppFormGenerator
        {
            Response = Result<GeneratedAppFormResult>.Failure(Error.Unexpected("AI generation failed: boom")),
        };
        var handler = new GenerateAppCommandHandler(generator, new FakeCurrentUserContext());

        var result = await handler.Handle(new GenerateAppCommand("A leave request form"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
