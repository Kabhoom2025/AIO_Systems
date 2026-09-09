using FlowSphere.Execution.Copilot;
using Xunit;

namespace FlowSphere.Tests.Execution;

public class AppFormShapeValidatorTests
{
    [Fact]
    public void TryValidate_WellFormedApp_Succeeds()
    {
        const string app = """
            {"name":"Leave Request","description":"Collects leave requests","sections":[
              {"title":"Details","fields":[
                {"key":"employee_name","type":"Text","label":"Employee Name","required":true},
                {"key":"leave_type","type":"Dropdown","label":"Leave Type","required":true,"options":["Sick","Vacation"]}
              ]}
            ]}
            """;

        Assert.True(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void TryValidate_MissingName_Fails()
    {
        const string app = """{"sections":[{"fields":[{"key":"a","type":"Text","label":"A"}]}]}""";

        Assert.False(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Contains("non-empty 'name'", error);
    }

    [Fact]
    public void TryValidate_NoSections_Fails()
    {
        const string app = """{"name":"Test","sections":[]}""";

        Assert.False(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Contains("no sections", error);
    }

    [Fact]
    public void TryValidate_SectionMissingFieldsArray_Fails()
    {
        const string app = """{"name":"Test","sections":[{"title":"Details"}]}""";

        Assert.False(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Contains("'fields' array", error);
    }

    [Fact]
    public void TryValidate_FieldWithUnsupportedType_Fails()
    {
        const string app = """{"name":"Test","sections":[{"fields":[{"key":"a","type":"Signature","label":"A"}]}]}""";

        Assert.False(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Contains("unsupported type", error);
    }

    [Fact]
    public void TryValidate_FieldMissingKey_Fails()
    {
        const string app = """{"name":"Test","sections":[{"fields":[{"type":"Text","label":"A"}]}]}""";

        Assert.False(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Contains("non-empty 'key' and 'label'", error);
    }

    [Fact]
    public void TryValidate_NoFieldsAnywhere_Fails()
    {
        const string app = """{"name":"Test","sections":[{"fields":[]}]}""";

        Assert.False(AppFormShapeValidator.TryValidate(app, out var error));
        Assert.Contains("no fields", error);
    }

    [Fact]
    public void TryValidate_InvalidJson_Fails()
    {
        Assert.False(AppFormShapeValidator.TryValidate("not json", out var error));
        Assert.Contains("not valid JSON", error);
    }
}
