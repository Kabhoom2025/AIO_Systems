namespace NovaERP.Application.DTOs;

public class TaxComponentDto
{
    public int     Id           { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public decimal RatePercent  { get; set; }
    public int     DisplayOrder { get; set; }
}

public class CreateTaxComponentDto
{
    public string  Name         { get; set; } = string.Empty;
    public decimal RatePercent  { get; set; }
    public int     DisplayOrder { get; set; }
}

public class TaxCodeDto
{
    public int    Id       { get; set; }
    public string Code     { get; set; } = string.Empty;
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; }
    public List<TaxComponentDto> Components { get; set; } = new();

    /// <summary>Sum of component rates — computed on read, not stored.</summary>
    public decimal TotalRatePercent { get; set; }
}

public class CreateTaxCodeDto
{
    public string Code     { get; set; } = string.Empty;
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
    public List<CreateTaxComponentDto> Components { get; set; } = new();
}

/// <summary>Components are managed as part of the tax code and replaced wholesale on
/// update — same "replace-all" approach as UpdateWorkflowDefinitionDto's Steps.</summary>
public class UpdateTaxCodeDto
{
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
    public List<CreateTaxComponentDto> Components { get; set; } = new();
}
