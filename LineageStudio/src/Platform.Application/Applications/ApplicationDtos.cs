namespace Platform.Application.Applications;

public record ApplicationDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsPublished,
    Guid? CurrentVersionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ApplicationVersionDto(
    Guid Id,
    Guid ApplicationId,
    int VersionNumber,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt);

public record CreateApplicationRequest(string Name, string? Description);

public record UpdateApplicationRequest(string Name, string? Description);
