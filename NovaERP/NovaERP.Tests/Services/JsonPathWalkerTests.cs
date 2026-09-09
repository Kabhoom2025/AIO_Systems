using System.Text.Json;
using System.Text.Json.Nodes;
using NovaERP.Infrastructure.Services;
using Xunit;

namespace NovaERP.Tests.Services;

public class JsonPathWalkerTests
{
    [Fact]
    public void SetValue_Simple_Dot_Path_Builds_Nested_Object()
    {
        var root = new JsonObject();
        JsonPathWalker.SetValue(root, "destination.zip", JsonValue.Create("560001"));

        Assert.Equal("560001", root["destination"]!["zip"]!.GetValue<string>());
    }

    [Fact]
    public void SetValue_Concrete_Array_Index_Builds_Array_Elements()
    {
        var root = new JsonObject();
        JsonPathWalker.SetValue(root, "packages[0].weight", JsonValue.Create(2.5m));
        JsonPathWalker.SetValue(root, "packages[1].weight", JsonValue.Create(4.0m));

        var packages = root["packages"]!.AsArray();
        Assert.Equal(2, packages.Count);
        Assert.Equal(2.5m, packages[0]!["weight"]!.GetValue<decimal>());
        Assert.Equal(4.0m, packages[1]!["weight"]!.GetValue<decimal>());
    }

    [Fact]
    public void GetValues_Simple_Dot_Path_Returns_Single_Value()
    {
        using var doc = JsonDocument.Parse("""{"destination":{"zip":"560001"}}""");
        var values = JsonPathWalker.GetValues(doc.RootElement, "destination.zip");

        Assert.Single(values);
        Assert.Equal("560001", values[0].GetString());
    }

    [Fact]
    public void GetValues_Wildcard_Path_Returns_One_Value_Per_Array_Element()
    {
        using var doc = JsonDocument.Parse("""{"rates":[{"price":10.5},{"price":22.0},{"price":15.75}]}""");
        var values = JsonPathWalker.GetValues(doc.RootElement, "rates[].price");

        Assert.Equal(3, values.Count);
        Assert.Equal(10.5m, values[0].GetDecimal());
        Assert.Equal(22.0m, values[1].GetDecimal());
        Assert.Equal(15.75m, values[2].GetDecimal());
    }

    [Fact]
    public void GetValues_Missing_Path_Returns_Empty_List()
    {
        using var doc = JsonDocument.Parse("""{"foo":"bar"}""");
        var values = JsonPathWalker.GetValues(doc.RootElement, "destination.zip");

        Assert.Empty(values);
    }

    [Fact]
    public void GetValues_Wildcard_On_Missing_Array_Returns_Empty_List()
    {
        using var doc = JsonDocument.Parse("""{"other":"value"}""");
        var values = JsonPathWalker.GetValues(doc.RootElement, "rates[].price");

        Assert.Empty(values);
    }

    [Fact]
    public void RoundTrip_Set_Then_Get_Produces_The_Same_Values()
    {
        var root = new JsonObject();
        JsonPathWalker.SetValue(root, "shipment.packages[0].weight", JsonValue.Create(1.2m));
        JsonPathWalker.SetValue(root, "shipment.packages[1].weight", JsonValue.Create(3.4m));

        using var doc = JsonDocument.Parse(root.ToJsonString());
        var values = JsonPathWalker.GetValues(doc.RootElement, "shipment.packages[].weight");

        Assert.Equal(2, values.Count);
        Assert.Equal(1.2m, values[0].GetDecimal());
        Assert.Equal(3.4m, values[1].GetDecimal());
    }
}
