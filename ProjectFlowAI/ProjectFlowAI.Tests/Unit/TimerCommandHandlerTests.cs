using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Features.TimeTracking;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Application.Mapping;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Tests.TestDoubles;
using Xunit;

namespace ProjectFlowAI.Tests.Unit;

public class TimerCommandHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    private static async Task<(Infrastructure.Data.ProjectFlowDbContext Db, WorkItem WorkItem, User User, FakeDateTimeProvider Clock, IMediator Mediator)> SetupAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var org = new Organization { Name = "Org", Slug = "org" };
        db.Organizations.Add(org);
        var user = new User { Email = "dev@example.com", PasswordHash = "x", FirstName = "D", LastName = "L" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var project = new Project { OrganizationId = org.Id, Key = "PFA", Name = "Test Project", OwnerUserId = user.Id };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var workItem = new WorkItem { ProjectId = project.Id, Title = "Do the thing", ReporterUserId = user.Id };
        db.WorkItems.Add(workItem);
        await db.SaveChangesAsync();

        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc) };

        // StopTimerCommandHandler re-sends LogWorkItemTimeCommand through IMediator (same code path
        // as Phase 2's manual time-log entry point), so tests need a real, minimal MediatR container
        // wired against the same InMemory db instance rather than a hand-rolled fake.
        var services = new ServiceCollection();
        services.AddSingleton<IProjectFlowDbContext>(db);
        services.AddSingleton(CreateMapper());
        services.AddSingleton<IDateTimeProvider>(clock);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PermissionCatalog).Assembly));
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        return (db, workItem, user, clock, mediator);
    }

    [Fact]
    public async Task Starting_A_Timer_When_None_Is_Running_Succeeds()
    {
        var (_, workItem, user, _, mediator) = await SetupAsync();

        var result = await mediator.Send(new StartTimerCommand(workItem.Id, user.Id));

        Assert.Equal(workItem.Id, result.WorkItemId);
        Assert.Equal("Do the thing", result.WorkItemTitle);
    }

    [Fact]
    public async Task Starting_A_Second_Timer_While_One_Is_Already_Running_Throws_Conflict()
    {
        var (_, workItem, user, _, mediator) = await SetupAsync();

        await mediator.Send(new StartTimerCommand(workItem.Id, user.Id));

        await Assert.ThrowsAsync<ConflictException>(() => mediator.Send(new StartTimerCommand(workItem.Id, user.Id)));
    }

    [Fact]
    public async Task Stopping_A_Timer_Computes_Elapsed_Minutes_And_Creates_A_Real_TimeLog()
    {
        var (db, workItem, user, clock, mediator) = await SetupAsync();

        await mediator.Send(new StartTimerCommand(workItem.Id, user.Id));
        clock.UtcNow = clock.UtcNow.AddMinutes(37);

        var timeLog = await mediator.Send(new StopTimerCommand(user.Id));

        Assert.Equal(37, timeLog.Minutes);
        Assert.Equal(workItem.Id, timeLog.WorkItemId);
        Assert.Empty(db.ActiveTimers);
        Assert.Single(db.WorkItemTimeLogs);
    }

    [Fact]
    public async Task Stopping_A_Timer_Immediately_Still_Logs_A_Nonzero_Minimum_Of_One_Minute()
    {
        var (db, workItem, user, _, mediator) = await SetupAsync();

        await mediator.Send(new StartTimerCommand(workItem.Id, user.Id));
        // No time advanced at all — must not create a nonsensical 0-minute entry.
        var timeLog = await mediator.Send(new StopTimerCommand(user.Id));

        Assert.Equal(1, timeLog.Minutes);
    }

    [Fact]
    public async Task Stopping_A_Timer_When_None_Is_Running_Throws_NotFound()
    {
        var (_, _, user, _, mediator) = await SetupAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => mediator.Send(new StopTimerCommand(user.Id)));
    }
}
