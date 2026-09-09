using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;
using ProjectFlowAI.Infrastructure.Services;

namespace ProjectFlowAI.Infrastructure.Data;

/// <summary>
/// Mirrors NovaERP.Infrastructure.Data.SeedData.SeedAsync: idempotent, safe to call on every
/// startup. Seeds the fixed 9-role system-role catalog, the full permission catalog, sensible
/// role->permission mappings, one demo Organization, and one demo user per role for login-testing.
/// </summary>
public static class SeedData
{
    public const string SuperAdmin = "SuperAdmin";
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string ProjectManager = "ProjectManager";
    public const string TeamLead = "TeamLead";
    public const string Developer = "Developer";
    public const string QAEngineer = "QAEngineer";
    public const string BusinessAnalyst = "BusinessAnalyst";
    public const string Client = "Client";
    public const string Guest = "Guest";

    private static readonly Dictionary<string, string[]> RolePermissionMap = new()
    {
        [SuperAdmin] = PermissionCatalog.All.Select(p => p.Key).ToArray(),
        [OrganizationAdmin] = new[]
        {
            PermissionCatalog.OrganizationsView, PermissionCatalog.DepartmentsManage, PermissionCatalog.DepartmentsView,
            PermissionCatalog.TeamsManage, PermissionCatalog.TeamsView, PermissionCatalog.UsersInvite,
            PermissionCatalog.UsersManage, PermissionCatalog.UsersView, PermissionCatalog.RolesManage,
            PermissionCatalog.RolesView, PermissionCatalog.AuditLogsView, PermissionCatalog.ProjectsManage,
            PermissionCatalog.ProjectsView, PermissionCatalog.TasksManage, PermissionCatalog.TasksView,
            PermissionCatalog.MilestonesManage, PermissionCatalog.CommentsManage, PermissionCatalog.ReportsView,
            PermissionCatalog.SprintsManage, PermissionCatalog.SprintsView,
            PermissionCatalog.ChatUse, PermissionCatalog.NotificationsManage, PermissionCatalog.DocsManage, PermissionCatalog.DocsView,
            PermissionCatalog.AiUse, PermissionCatalog.AutomationManage
        },
        [ProjectManager] = new[]
        {
            PermissionCatalog.TeamsView, PermissionCatalog.UsersView, PermissionCatalog.ProjectsManage,
            PermissionCatalog.ProjectsView, PermissionCatalog.TasksManage, PermissionCatalog.TasksView,
            PermissionCatalog.MilestonesManage, PermissionCatalog.CommentsManage, PermissionCatalog.ReportsView,
            PermissionCatalog.SprintsManage, PermissionCatalog.SprintsView,
            PermissionCatalog.ChatUse, PermissionCatalog.DocsManage, PermissionCatalog.DocsView,
            PermissionCatalog.AiUse, PermissionCatalog.AutomationManage
        },
        [TeamLead] = new[]
        {
            PermissionCatalog.TeamsView, PermissionCatalog.UsersView, PermissionCatalog.ProjectsManage,
            PermissionCatalog.ProjectsView, PermissionCatalog.TasksManage, PermissionCatalog.TasksView,
            PermissionCatalog.MilestonesManage, PermissionCatalog.CommentsManage,
            PermissionCatalog.SprintsManage, PermissionCatalog.SprintsView,
            PermissionCatalog.ChatUse, PermissionCatalog.DocsManage, PermissionCatalog.DocsView,
            PermissionCatalog.AiUse, PermissionCatalog.AutomationManage
        },
        [Developer] = new[]
        {
            PermissionCatalog.ProjectsView, PermissionCatalog.TasksManage, PermissionCatalog.TasksView,
            PermissionCatalog.CommentsManage, PermissionCatalog.UsersView, PermissionCatalog.SprintsView,
            PermissionCatalog.ChatUse, PermissionCatalog.DocsView, PermissionCatalog.AiUse
        },
        [QAEngineer] = new[]
        {
            PermissionCatalog.ProjectsView, PermissionCatalog.TasksManage, PermissionCatalog.TasksView,
            PermissionCatalog.CommentsManage, PermissionCatalog.UsersView, PermissionCatalog.SprintsView,
            PermissionCatalog.ChatUse, PermissionCatalog.DocsView, PermissionCatalog.AiUse
        },
        [BusinessAnalyst] = new[]
        {
            PermissionCatalog.ProjectsView, PermissionCatalog.TasksManage, PermissionCatalog.TasksView,
            PermissionCatalog.CommentsManage, PermissionCatalog.ReportsView, PermissionCatalog.UsersView,
            PermissionCatalog.SprintsView, PermissionCatalog.ChatUse, PermissionCatalog.DocsView, PermissionCatalog.AiUse
        },
        [Client] = new[] { PermissionCatalog.ProjectsView, PermissionCatalog.TasksView, PermissionCatalog.SprintsView, PermissionCatalog.ChatUse, PermissionCatalog.DocsView, PermissionCatalog.AiUse },
        [Guest] = new[] { PermissionCatalog.ProjectsView, PermissionCatalog.TasksView, PermissionCatalog.SprintsView, PermissionCatalog.ChatUse, PermissionCatalog.DocsView, PermissionCatalog.AiUse },
    };

    /// <summary>role name -> (email, password) — kept in sync with the README's demo-login table.</summary>
    public static readonly Dictionary<string, (string Email, string Password)> DemoLogins = new()
    {
        [SuperAdmin] = ("superadmin@projectflow.local", "Super@123"),
        [OrganizationAdmin] = ("admin@projectflow.local", "Admin@123"),
        [ProjectManager] = ("pm@projectflow.local", "Pm@123456"),
        [TeamLead] = ("teamlead@projectflow.local", "Teamlead@123"),
        [Developer] = ("dev@projectflow.local", "Dev@123456"),
        [QAEngineer] = ("qa@projectflow.local", "Qa@123456"),
        [BusinessAnalyst] = ("ba@projectflow.local", "Ba@123456"),
        [Client] = ("client@projectflow.local", "Client@123"),
        [Guest] = ("guest@projectflow.local", "Guest@123"),
    };

    public static async Task SeedAsync(ProjectFlowDbContext db)
    {
        var hasher = new PasswordHasherService();

        // Permissions
        var existingPermissionKeys = await db.Permissions.Select(p => p.Key).ToListAsync();
        foreach (var def in PermissionCatalog.All)
        {
            if (!existingPermissionKeys.Contains(def.Key))
                db.Permissions.Add(new Permission { Key = def.Key, Description = def.Description, Category = def.Category });
        }
        await db.SaveChangesAsync();

        var permissionsByKey = await db.Permissions.ToDictionaryAsync(p => p.Key);

        // System roles (OrganizationId == null => available to every org)
        var existingRoleNames = await db.Roles.Where(r => r.IsSystemRole).Select(r => r.Name).ToListAsync();
        foreach (var roleName in RolePermissionMap.Keys)
        {
            if (existingRoleNames.Contains(roleName)) continue;
            db.Roles.Add(new Role { Name = roleName, IsSystemRole = true, OrganizationId = null });
        }
        await db.SaveChangesAsync();

        var rolesByName = await db.Roles.Where(r => r.IsSystemRole).ToDictionaryAsync(r => r.Name);

        foreach (var (roleName, permissionKeys) in RolePermissionMap)
        {
            var role = rolesByName[roleName];
            var existingMappedPermissionIds = await db.RolePermissions
                .Where(rp => rp.RoleId == role.Id).Select(rp => rp.PermissionId).ToListAsync();

            foreach (var key in permissionKeys)
            {
                if (!permissionsByKey.TryGetValue(key, out var permission)) continue;
                if (existingMappedPermissionIds.Contains(permission.Id)) continue;
                db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }
        await db.SaveChangesAsync();

        // Demo organization
        var org = await db.Organizations.FirstOrDefaultAsync(o => o.Slug == "projectflow-demo");
        if (org == null)
        {
            org = new Organization
            {
                Name = "ProjectFlow Demo Org",
                Slug = "projectflow-demo",
                SubscriptionPlan = SubscriptionPlan.Business,
                IsActive = true
            };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
        }

        // One demo user per role
        foreach (var (roleName, (email, password)) in DemoLogins)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    PasswordHash = hasher.HashPassword(password),
                    FirstName = roleName,
                    LastName = "Demo",
                    Status = UserStatus.Active,
                    IsEmailVerified = true,
                    // SuperAdmin is platform-wide (OrganizationId null); everyone else belongs to the demo org.
                    OrganizationId = roleName == SuperAdmin ? null : org.Id
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            var role = rolesByName[roleName];
            var alreadyAssigned = await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            if (!alreadyAssigned)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    OrganizationId = roleName == SuperAdmin ? null : org.Id
                });
                await db.SaveChangesAsync();
            }
        }

        await SeedDemoProjectAsync(db, org);
        await SeedDemoChatNotificationsAndWikiAsync(db, org);
    }

    /// <summary>Phase 4: one demo org-wide chat channel with a couple of seeded messages, a couple
    /// of demo notifications, and a couple of demo wiki pages, so the frontend has real data. Uses
    /// its own idempotency check (rather than relying on SeedDemoProjectAsync's early-return) —
    /// exactly the bug class called out in SeedDemoProjectAsync's own comment above.</summary>
    private static async Task SeedDemoChatNotificationsAndWikiAsync(ProjectFlowDbContext db, Organization org)
    {
        var usersByEmail = await db.Users.Where(u => u.OrganizationId == org.Id).ToDictionaryAsync(u => u.Email);
        var pm = usersByEmail[DemoLogins[ProjectManager].Email];
        var dev = usersByEmail[DemoLogins[Developer].Email];
        var qa = usersByEmail[DemoLogins[QAEngineer].Email];

        if (!await db.ChatChannels.AnyAsync(c => c.OrganizationId == org.Id && c.Name == "general"))
        {
            var channel = new ChatChannel { OrganizationId = org.Id, Name = "general", IsPrivate = false, CreatedByUserId = pm.Id };
            db.ChatChannels.Add(channel);
            await db.SaveChangesAsync();

            db.ChatChannelMembers.AddRange(
                new ChatChannelMember { ChannelId = channel.Id, UserId = pm.Id },
                new ChatChannelMember { ChannelId = channel.Id, UserId = dev.Id },
                new ChatChannelMember { ChannelId = channel.Id, UserId = qa.Id });

            var firstMessage = new ChatMessage { ChannelId = channel.Id, AuthorUserId = pm.Id, Body = "Welcome to the ProjectFlow AI demo org chat!" };
            db.ChatMessages.Add(firstMessage);
            await db.SaveChangesAsync();

            db.ChatMessages.Add(new ChatMessage
            {
                ChannelId = channel.Id, AuthorUserId = dev.Id, Body = "Thanks! Excited to get started.",
                ParentMessageId = firstMessage.Id
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Notifications.AnyAsync(n => n.UserId == dev.Id))
        {
            db.Notifications.AddRange(
                new Notification
                {
                    UserId = dev.Id, Type = Domain.NotificationType.Mention, Title = "You were mentioned in a comment",
                    Body = "pm@projectflow.local mentioned you on \"Implement WorkItem CRUD API\".", LinkUrl = "/projects", IsRead = false
                },
                new Notification
                {
                    UserId = dev.Id, Type = Domain.NotificationType.General, Title = "Welcome to ProjectFlow AI",
                    Body = "Your account is set up and ready to go.", IsRead = true
                });
            await db.SaveChangesAsync();
        }

        if (!await db.DocPages.AnyAsync(p => p.OrganizationId == org.Id))
        {
            var welcome = new DocPage
            {
                OrganizationId = org.Id, Scope = Domain.DocScope.OrgWiki, Category = Domain.DocCategory.KnowledgeBase,
                Title = "Welcome to the ProjectFlow AI Wiki", Content = "<h1>Welcome</h1><p>This is the org-wide knowledge base.</p>",
                CreatedByUserId = pm.Id
            };
            db.DocPages.Add(welcome);
            await db.SaveChangesAsync();

            db.DocPages.Add(new DocPage
            {
                OrganizationId = org.Id, Scope = Domain.DocScope.OrgWiki, Category = Domain.DocCategory.MeetingNotes,
                Title = "Sprint Planning Notes — Sprint 1", Content = "<h2>Sprint 1 Planning</h2><p>Agreed scope for Sprint 1.</p>",
                ParentPageId = welcome.Id, CreatedByUserId = pm.Id
            });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>Phase 2: one demo Project with a handful of WorkItems spread across statuses,
    /// priorities and assignees (drawn from the 9 demo users) so the frontend has real Kanban data
    /// to render immediately after Phase 1's org/role seed above.</summary>
    private static async Task SeedDemoProjectAsync(ProjectFlowDbContext db, Organization org)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.OrganizationId == org.Id && p.Key == "PFA");
        if (project != null)
        {
            // Phase 2's project/work-items already exist from an earlier run of this same
            // idempotent seeder — still make sure Phase 3's demo Sprint gets seeded on top of them
            // (the original "return early" here meant SeedDemoSprintAsync below was silently never
            // reached once the DB already had Phase 2 data, which is exactly this app's own bug class).
            var existingWorkItems = await db.WorkItems.Where(w => w.ProjectId == project.Id)
                .OrderBy(w => w.CreatedAt).ToArrayAsync();
            await SeedDemoSprintAsync(db, project, existingWorkItems);
            return;
        }

        var usersByEmail = await db.Users.Where(u => u.OrganizationId == org.Id).ToDictionaryAsync(u => u.Email);
        var pm = usersByEmail[DemoLogins[ProjectManager].Email];
        var dev = usersByEmail[DemoLogins[Developer].Email];
        var qa = usersByEmail[DemoLogins[QAEngineer].Email];
        var teamLead = usersByEmail[DemoLogins[TeamLead].Email];
        var ba = usersByEmail[DemoLogins[BusinessAnalyst].Email];

        project = new Project
        {
            OrganizationId = org.Id,
            Key = "PFA",
            Name = "ProjectFlow AI Launch",
            Description = "Internal demo project seeded for Phase 2 (Projects/Milestones/WorkItems/Kanban).",
            Status = ProjectStatus.Active,
            StartDate = DateTime.UtcNow.Date.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(60),
            OwnerUserId = pm.Id
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        db.ProjectMembers.AddRange(
            new ProjectMember { ProjectId = project.Id, UserId = pm.Id, RoleInProject = "Lead" },
            new ProjectMember { ProjectId = project.Id, UserId = dev.Id, RoleInProject = "Contributor" },
            new ProjectMember { ProjectId = project.Id, UserId = qa.Id, RoleInProject = "Contributor" },
            new ProjectMember { ProjectId = project.Id, UserId = teamLead.Id, RoleInProject = "Contributor" },
            new ProjectMember { ProjectId = project.Id, UserId = ba.Id, RoleInProject = "Contributor" });

        db.Milestones.AddRange(
            new Milestone { ProjectId = project.Id, Name = "Beta Release", Description = "Feature-complete beta.", DueDate = DateTime.UtcNow.Date.AddDays(21), Status = MilestoneStatus.Open },
            new Milestone { ProjectId = project.Id, Name = "GA Launch", Description = "General availability.", DueDate = DateTime.UtcNow.Date.AddDays(60), Status = MilestoneStatus.Open });

        var bugLabel = new Label { ProjectId = project.Id, Name = "bug", ColorHex = "#EF4444" };
        var featureLabel = new Label { ProjectId = project.Id, Name = "feature", ColorHex = "#6366F1" };
        var urgentLabel = new Label { ProjectId = project.Id, Name = "urgent", ColorHex = "#F59E0B" };
        db.Labels.AddRange(bugLabel, featureLabel, urgentLabel);
        await db.SaveChangesAsync();

        var workItems = new[]
        {
            new WorkItem { ProjectId = project.Id, Title = "Design Kanban board schema", Description = "Define WorkItemStatus columns and drag-drop position strategy.", Status = WorkItemStatus.Done, Priority = WorkItemPriority.High, Type = WorkItemType.Task, AssigneeUserId = dev.Id, ReporterUserId = pm.Id, StoryPoints = 5, EstimatedHours = 8, Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "Implement WorkItem CRUD API", Description = "CreateWorkItem/UpdateWorkItem/DeleteWorkItem endpoints.", Status = WorkItemStatus.InProgress, Priority = WorkItemPriority.High, Type = WorkItemType.Story, AssigneeUserId = dev.Id, ReporterUserId = pm.Id, StoryPoints = 8, EstimatedHours = 16, Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "Fix drag-drop position rounding bug", Description = "Position sometimes collides after many reorders.", Status = WorkItemStatus.ToDo, Priority = WorkItemPriority.Highest, Type = WorkItemType.Bug, AssigneeUserId = dev.Id, ReporterUserId = qa.Id, StoryPoints = 2, EstimatedHours = 3, Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "Write Kanban board integration tests", Description = "Cover GetKanbanBoard grouping/sorting.", Status = WorkItemStatus.CodeReview, Priority = WorkItemPriority.Medium, Type = WorkItemType.Task, AssigneeUserId = qa.Id, ReporterUserId = teamLead.Id, StoryPoints = 3, EstimatedHours = 5, Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "QA pass on recurring task auto-creation", Description = "Verify next occurrence clones correctly when moved to Done.", Status = WorkItemStatus.Testing, Priority = WorkItemPriority.Medium, Type = WorkItemType.Task, AssigneeUserId = qa.Id, ReporterUserId = pm.Id, StoryPoints = 3, EstimatedHours = 4, Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "Blocked: waiting on design review for custom fields UI", Description = "Custom field definitions need UX sign-off before frontend work starts.", Status = WorkItemStatus.Blocked, Priority = WorkItemPriority.Low, Type = WorkItemType.Task, AssigneeUserId = ba.Id, ReporterUserId = pm.Id, StoryPoints = 2, EstimatedHours = 2, Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "Draft weekly status report template", Description = "Recurring reporting task.", Status = WorkItemStatus.Backlog, Priority = WorkItemPriority.Low, Type = WorkItemType.Task, AssigneeUserId = ba.Id, ReporterUserId = pm.Id, StoryPoints = 1, EstimatedHours = 1, IsRecurring = true, RecurrenceIntervalDays = 7, DueDate = DateTime.UtcNow.Date.AddDays(7), Position = 1024 },
            new WorkItem { ProjectId = project.Id, Title = "Investigate slow ListProjects query under load", Description = "Profile paging query with large seed data.", Status = WorkItemStatus.Backlog, Priority = WorkItemPriority.Medium, Type = WorkItemType.Bug, AssigneeUserId = null, ReporterUserId = teamLead.Id, StoryPoints = 3, EstimatedHours = 6, Position = 2048 },
        };
        db.WorkItems.AddRange(workItems);
        await db.SaveChangesAsync();

        db.WorkItemLabels.AddRange(
            new WorkItemLabel { WorkItemId = workItems[2].Id, LabelId = bugLabel.Id },
            new WorkItemLabel { WorkItemId = workItems[2].Id, LabelId = urgentLabel.Id },
            new WorkItemLabel { WorkItemId = workItems[1].Id, LabelId = featureLabel.Id },
            new WorkItemLabel { WorkItemId = workItems[7].Id, LabelId = bugLabel.Id });

        db.WorkItemActivities.AddRange(workItems.Select(w => new WorkItemActivity
        {
            WorkItemId = w.Id, UserId = w.ReporterUserId, Action = "Created"
        }));

        db.WorkItemTimeLogs.Add(new WorkItemTimeLog
        {
            WorkItemId = workItems[0].Id, UserId = dev.Id, Minutes = 240, Note = "Initial schema design", LoggedDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-2))
        });

        await db.SaveChangesAsync();

        await SeedDemoSprintAsync(db, project, workItems);
    }

    /// <summary>Phase 3: one Active demo Sprint containing a few of the already-seeded WorkItems,
    /// so the frontend's Scrum board/burndown/burnup/retrospective views have real data immediately.</summary>
    private static async Task SeedDemoSprintAsync(ProjectFlowDbContext db, Project project, WorkItem[] workItems)
    {
        if (await db.Sprints.AnyAsync(s => s.ProjectId == project.Id)) return;

        var sprint = new Sprint
        {
            ProjectId = project.Id,
            Name = "Sprint 1",
            Goal = "Ship the Kanban board MVP end-to-end.",
            StartDate = DateTime.UtcNow.Date.AddDays(-7),
            EndDate = DateTime.UtcNow.Date.AddDays(7),
            Status = SprintStatus.Active
        };
        db.Sprints.Add(sprint);
        await db.SaveChangesAsync();

        // Pull the first four seeded work items (Done/InProgress/ToDo/CodeReview) into the sprint
        // and give the Gantt/scrum board something realistic to render: real StartDate values.
        foreach (var item in workItems.Take(4))
        {
            item.SprintId = sprint.Id;
            item.StartDate ??= item.CreatedAt.Date;
        }
        await db.SaveChangesAsync();
    }
}
