using ProjectFlowAI.Application.Features.Ai;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

/// <summary>The primary approach to getting clean JSON out of Claude is a strict system-prompt
/// instruction; StripFences/Parse are the defensive fallback for when the model doesn't fully
/// comply. These tests cover both: a well-formed response parses correctly, and a response wrapped
/// in a ```json fence still parses via the defensive stripping.</summary>
public class AiJsonParsingTests
{
    private record Sample(string Title, int Count);

    [Fact]
    public void Parse_Well_Formed_Json_Response_Parses_Correctly()
    {
        var raw = """{"title":"Hello","count":3}""";
        var result = AiJson.Parse<Sample>(raw);

        Assert.Equal("Hello", result.Title);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Parse_Response_Wrapped_In_Json_Fence_Still_Parses()
    {
        var raw = "```json\n{\"title\":\"Fenced\",\"count\":7}\n```";
        var result = AiJson.Parse<Sample>(raw);

        Assert.Equal("Fenced", result.Title);
        Assert.Equal(7, result.Count);
    }

    [Fact]
    public void Parse_Response_Wrapped_In_Bare_Fence_Without_Language_Tag_Still_Parses()
    {
        var raw = "```\n{\"title\":\"Bare\",\"count\":1}\n```";
        var result = AiJson.Parse<Sample>(raw);

        Assert.Equal("Bare", result.Title);
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public void StripFences_Leaves_Unfenced_Text_Unchanged()
    {
        var raw = """{"title":"Plain","count":5}""";
        Assert.Equal(raw, AiJson.StripFences(raw));
    }

    [Fact]
    public void Parse_Array_Response_Parses_Correctly()
    {
        var raw = """[{"title":"A","count":1},{"title":"B","count":2}]""";
        var result = AiJson.Parse<List<Sample>>(raw);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Title);
        Assert.Equal("B", result[1].Title);
    }
}
