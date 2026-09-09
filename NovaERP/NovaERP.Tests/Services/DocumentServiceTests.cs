using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaERP.Application.Interfaces;
using NovaERP.Application.Mapping;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;
using NovaERP.Infrastructure.Repositories;
using NovaERP.Infrastructure.Services;
using Xunit;

namespace NovaERP.Tests.Services;

public class DocumentServiceTests
{
    private class NoopFileStorageService : IFileStorageService
    {
        public Task<string> SaveAsync(Stream content, string fileName, string organizationCode) =>
            Task.FromResult($"{organizationCode}/{fileName}");

        public Task<Stream> OpenReadAsync(string storagePath) => Task.FromResult<Stream>(new MemoryStream());

        public Task DeleteAsync(string storagePath) => Task.CompletedTask;
    }

    private static NovaErpDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NovaErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NovaErpDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static IConfiguration CreateConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["FileStorage:MaxSizeBytes"] = (5 * 1024 * 1024).ToString() // 5 MB test limit
        })
        .Build();

    private static DocumentService CreateService(NovaErpDbContext ctx) =>
        new(new DocumentRepository(ctx), new NoopFileStorageService(), CreateMapper(), CreateConfig());

    [Fact]
    public async Task UploadAsync_Rejects_File_Larger_Than_Configured_Limit()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));
        const long oversizedBytes = 6L * 1024 * 1024; // over the 5 MB test limit

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(ctx).UploadAsync(org.Id, 1, content, "big.pdf", "application/pdf", oversizedBytes, null, null));
    }

    [Fact]
    public async Task UploadAsync_Rejects_Disallowed_Content_Type()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(ctx).UploadAsync(org.Id, 1, content, "script.exe", "application/x-msdownload", 1024, null, null));
    }

    [Fact]
    public async Task UploadAsync_Accepts_Valid_File_And_Persists_It()
    {
        var ctx = CreateContext();
        var org = new Organization { Name = "Acme", Code = "ACME" };
        ctx.Organizations.Add(org);
        await ctx.SaveChangesAsync();

        using var content = new MemoryStream(Encoding.UTF8.GetBytes("dummy"));

        var result = await CreateService(ctx).UploadAsync(org.Id, 1, content, "invoice.pdf", "application/pdf", 1024, "Branch", 7);

        Assert.Equal("invoice.pdf", result.FileName);
        Assert.Single(await ctx.Documents.ToListAsync());
    }
}
