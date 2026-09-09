namespace ProjectFlowAI.Domain.Entities;

/// <summary>A rich-text (HTML) document page. ProjectId null + Scope==OrgWiki means an org-wide wiki
/// page; ProjectId set + Scope==ProjectDocument means a project-scoped document. Category is only
/// meaningful for OrgWiki pages.</summary>
public class DocPage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public DocScope Scope { get; set; } = DocScope.OrgWiki;
    public DocCategory? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentPageId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Organization? Organization { get; set; }
    public Project? Project { get; set; }
    public DocPage? ParentPage { get; set; }
    public User? CreatedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
    public ICollection<DocPage> ChildPages { get; set; } = new List<DocPage>();
    public ICollection<DocPageVersion> Versions { get; set; } = new List<DocPageVersion>();
    public ICollection<DocPageComment> Comments { get; set; } = new List<DocPageComment>();
}

/// <summary>A snapshot of a page's Content taken immediately before an edit overwrites it — history
/// is append-only, never overwritten (see UpdateDocPageCommandHandler).</summary>
public class DocPageVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PageId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public Guid EditedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DocPage? Page { get; set; }
    public User? EditedByUser { get; set; }
}

public class DocPageComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PageId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DocPage? Page { get; set; }
    public User? AuthorUser { get; set; }
}
