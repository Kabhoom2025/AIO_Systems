using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectFlowAI.Application.Features.DocPages;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class DocPageVersioningTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static async Task<(Infrastructure.Data.ProjectFlowDbContext Db, Organization Org, User Author)> CreateOrgAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var author = new User { Email = "author@example.com", PasswordHash = "x", FirstName = "Au", LastName = "Thor", OrganizationId = org.Id };
        db.Users.Add(author);
        await db.SaveChangesAsync();
        return (db, org, author);
    }

    [Fact]
    public async Task Editing_A_Page_Creates_Exactly_One_New_Version_Row_With_The_Old_Content()
    {
        var (db, org, author) = await CreateOrgAsync();
        var page = new DocPage
        {
            OrganizationId = org.Id, Scope = DocScope.OrgWiki, Title = "Page", Content = "<p>Original content</p>", CreatedByUserId = author.Id
        };
        db.DocPages.Add(page);
        await db.SaveChangesAsync();

        var handler = new UpdateDocPageCommandHandler(db, CreateMapper());
        await handler.Handle(new UpdateDocPageCommand(page.Id, "Page", "<p>New content</p>", null, null, author.Id), CancellationToken.None);

        var versions = await db.DocPageVersions.Where(v => v.PageId == page.Id).ToListAsync();
        Assert.Single(versions);
        Assert.Equal("<p>Original content</p>", versions[0].Content);
        Assert.Equal(1, versions[0].VersionNumber);

        var reloaded = await db.DocPages.FirstAsync(p => p.Id == page.Id);
        Assert.Equal("<p>New content</p>", reloaded.Content);
    }

    [Fact]
    public async Task Editing_A_Page_Twice_Creates_Two_Sequential_Version_Rows()
    {
        var (db, org, author) = await CreateOrgAsync();
        var page = new DocPage { OrganizationId = org.Id, Scope = DocScope.OrgWiki, Title = "Page", Content = "v1", CreatedByUserId = author.Id };
        db.DocPages.Add(page);
        await db.SaveChangesAsync();

        var handler = new UpdateDocPageCommandHandler(db, CreateMapper());
        await handler.Handle(new UpdateDocPageCommand(page.Id, "Page", "v2", null, null, author.Id), CancellationToken.None);
        await handler.Handle(new UpdateDocPageCommand(page.Id, "Page", "v3", null, null, author.Id), CancellationToken.None);

        var versions = await db.DocPageVersions.Where(v => v.PageId == page.Id).OrderBy(v => v.VersionNumber).ToListAsync();
        Assert.Equal(2, versions.Count);
        Assert.Equal("v1", versions[0].Content);
        Assert.Equal("v2", versions[1].Content);
        Assert.Equal(1, versions[0].VersionNumber);
        Assert.Equal(2, versions[1].VersionNumber);
    }

    [Fact]
    public async Task Restoring_A_Version_Overwrites_Current_Content_And_Snapshots_The_Prior_Current_Content_As_A_New_Version()
    {
        var (db, org, author) = await CreateOrgAsync();
        var page = new DocPage { OrganizationId = org.Id, Scope = DocScope.OrgWiki, Title = "Page", Content = "v1", CreatedByUserId = author.Id };
        db.DocPages.Add(page);
        await db.SaveChangesAsync();

        var updateHandler = new UpdateDocPageCommandHandler(db, CreateMapper());
        await updateHandler.Handle(new UpdateDocPageCommand(page.Id, "Page", "v2", null, null, author.Id), CancellationToken.None);

        var v1 = await db.DocPageVersions.FirstAsync(v => v.PageId == page.Id && v.VersionNumber == 1);

        var restoreHandler = new RestoreDocPageVersionCommandHandler(db, CreateMapper());
        var result = await restoreHandler.Handle(new RestoreDocPageVersionCommand(page.Id, v1.Id, author.Id), CancellationToken.None);

        Assert.Equal("v1", result.Content);

        var versions = await db.DocPageVersions.Where(v => v.PageId == page.Id).OrderBy(v => v.VersionNumber).ToListAsync();
        Assert.Equal(2, versions.Count);
        Assert.Equal("v2", versions[1].Content); // the pre-restore current content ("v2") was snapshotted
    }
}
