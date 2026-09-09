using FlowSphere.Execution.Copilot;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class GraphShapeValidatorTests
{
    [Fact]
    public void TryValidate_LinearGraph_Succeeds()
    {
        const string graph = """
            {"nodes":[{"key":"t1","type":"Trigger","config":{}},{"key":"h1","type":"HttpRequest","config":{}}],
             "edges":[{"source":"t1","target":"h1"}]}
            """;

        Assert.True(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_NoTriggerNode_Fails()
    {
        const string graph = """{"nodes":[{"key":"h1","type":"HttpRequest","config":{}}],"edges":[]}""";

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("exactly one Trigger node", error);
    }

    [Fact]
    public void TryValidate_TriggerHasIncomingEdge_Fails()
    {
        const string graph = """
            {"nodes":[{"key":"t1","type":"Trigger","config":{}},{"key":"h1","type":"HttpRequest","config":{}}],
             "edges":[{"source":"h1","target":"t1"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("must not have any incoming edges", error);
    }

    [Fact]
    public void TryValidate_DuplicateNodeKey_Fails()
    {
        const string graph = """
            {"nodes":[{"key":"t1","type":"Trigger","config":{}},{"key":"t1","type":"HttpRequest","config":{}}],
             "edges":[]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("Duplicate node key", error);
    }

    [Fact]
    public void TryValidate_EdgeReferencesUnknownNode_Fails()
    {
        const string graph = """
            {"nodes":[{"key":"t1","type":"Trigger","config":{}}],
             "edges":[{"source":"t1","target":"ghost"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("unknown target node 'ghost'", error);
    }

    [Fact]
    public void TryValidate_DecisionMissingEdgeForCaseHandle_Fails()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"d1","type":"Decision","config":{"field":"x","operator":"equals","cases":[{"value":"a","handle":"caseA"}],"defaultHandle":"caseDefault"}},
                {"key":"h1","type":"HttpRequest","config":{}}
             ],
             "edges":[{"source":"t1","target":"d1"},{"source":"d1","target":"h1","sourceHandle":"caseDefault"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("no outgoing edge for handle 'caseA'", error);
    }

    [Fact]
    public void TryValidate_DecisionWithAllHandlesConnected_Succeeds()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"d1","type":"Decision","config":{"field":"x","operator":"equals","cases":[{"value":"a","handle":"caseA"}],"defaultHandle":"caseDefault"}},
                {"key":"h1","type":"HttpRequest","config":{}},
                {"key":"h2","type":"HttpRequest","config":{}}
             ],
             "edges":[
                {"source":"t1","target":"d1"},
                {"source":"d1","target":"h1","sourceHandle":"caseA"},
                {"source":"d1","target":"h2","sourceHandle":"caseDefault"}
             ]}
            """;

        Assert.True(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_LoopWithMultipleOutgoingEdges_Fails()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"l1","type":"Loop","config":{"arrayPath":"items"}},
                {"key":"h1","type":"HttpRequest","config":{}},
                {"key":"h2","type":"HttpRequest","config":{}}
             ],
             "edges":[{"source":"t1","target":"l1"},{"source":"l1","target":"h1"},{"source":"l1","target":"h2"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("exactly one outgoing edge", error);
    }

    [Fact]
    public void TryValidate_ParallelMissingMergeNodeKey_Fails()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"p1","type":"Parallel","config":{}},
                {"key":"h1","type":"HttpRequest","config":{}},
                {"key":"h2","type":"HttpRequest","config":{}}
             ],
             "edges":[{"source":"t1","target":"p1"},{"source":"p1","target":"h1"},{"source":"p1","target":"h2"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("missing 'mergeNodeKey'", error);
    }

    [Fact]
    public void TryValidate_ParallelMergeNodeKeyPointsToWrongType_Fails()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"p1","type":"Parallel","config":{"mergeNodeKey":"h3"}},
                {"key":"h1","type":"HttpRequest","config":{}},
                {"key":"h2","type":"HttpRequest","config":{}},
                {"key":"h3","type":"HttpRequest","config":{}}
             ],
             "edges":[{"source":"t1","target":"p1"},{"source":"p1","target":"h1"},{"source":"p1","target":"h2"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("must refer to a Merge node", error);
    }

    [Fact]
    public void TryValidate_ParallelWithValidMergeNodeAndBranches_Succeeds()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"p1","type":"Parallel","config":{"mergeNodeKey":"m1"}},
                {"key":"h1","type":"HttpRequest","config":{}},
                {"key":"h2","type":"HttpRequest","config":{}},
                {"key":"m1","type":"Merge","config":{}}
             ],
             "edges":[
                {"source":"t1","target":"p1"},
                {"source":"p1","target":"h1"},
                {"source":"p1","target":"h2"},
                {"source":"h1","target":"m1"},
                {"source":"h2","target":"m1"}
             ]}
            """;

        Assert.True(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_UserTaskMissingRejectedHandle_Fails()
    {
        const string graph = """
            {"nodes":[
                {"key":"t1","type":"Trigger","config":{}},
                {"key":"u1","type":"UserTask","config":{}},
                {"key":"h1","type":"HttpRequest","config":{}}
             ],
             "edges":[{"source":"t1","target":"u1"},{"source":"u1","target":"h1","sourceHandle":"approved"}]}
            """;

        Assert.False(GraphShapeValidator.TryValidate(graph, out var error));
        Assert.Contains("'approved' and 'rejected'", error);
    }

    [Fact]
    public void TryValidate_MalformedJson_Fails()
    {
        Assert.False(GraphShapeValidator.TryValidate("not json at all", out var error));
        Assert.Contains("not valid JSON", error);
    }
}
