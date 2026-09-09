using FlowSphere.Application.Interfaces;
using FlowSphere.Execution.Nodes;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class ConditionNodeExecutorTests
{
    private readonly ConditionNodeExecutor _executor = new();

    private static NodeExecutionContext Context(string configJson, string inputJson) => new()
    {
        OrganizationId = 1,
        NodeKey = "cond1",
        ConfigJson = configJson,
        InputJson = inputJson,
        PriorOutputs = new Dictionary<string, string?>(),
    };

    [Fact]
    public async Task Equals_Match_ReturnsTrueHandle()
    {
        var result = await _executor.ExecuteAsync(
            Context("""{"field":"status","operator":"equals","value":"ok"}""", """{"status":"ok"}"""),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("true", result.NextHandle);
    }

    [Fact]
    public async Task Equals_NoMatch_ReturnsFalseHandle()
    {
        var result = await _executor.ExecuteAsync(
            Context("""{"field":"status","operator":"equals","value":"ok"}""", """{"status":"error"}"""),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("false", result.NextHandle);
    }

    [Fact]
    public async Task NestedField_DotPath_Resolves()
    {
        var result = await _executor.ExecuteAsync(
            Context("""{"field":"body.statusCode","operator":"equals","value":"200"}""", """{"body":{"statusCode":"200"}}"""),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("true", result.NextHandle);
    }

    [Fact]
    public async Task GreaterThan_NumericComparison_Works()
    {
        var result = await _executor.ExecuteAsync(
            Context("""{"field":"count","operator":"greaterThan","value":"5"}""", """{"count":"10"}"""),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("true", result.NextHandle);
    }

    [Fact]
    public async Task MissingField_TreatedAsNoMatch_NotAsError()
    {
        var result = await _executor.ExecuteAsync(
            Context("""{"field":"missing.path","operator":"equals","value":"x"}""", """{"status":"ok"}"""),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("false", result.NextHandle);
    }
}
