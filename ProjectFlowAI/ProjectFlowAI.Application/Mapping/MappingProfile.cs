using AutoMapper;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // No CreateMap<User, UserDto> — Roles/Permissions require a join across
        // UserRoles/Roles/RolePermissions that isn't a 1:1 property on User, so every
        // UserDto is built by hand (ListUsersQueryHandler, AuthCommands.ToDto) with the
        // caller supplying already-computed roles/permissions, never via _mapper.Map/ProjectTo.
        CreateMap<Organization, OrganizationDto>();
        CreateMap<Department, DepartmentDto>();
        // These four DTOs are records with a primary constructor, so a projected member
        // (one that isn't a 1:1 property match on the source entity) must be wired via
        // ForCtorParam rather than ForMember — ForMember targets a settable property setter,
        // which records don't have; using it here made AutoMapper fall back to requiring a
        // parameterless constructor, which records also don't have, and blew up at runtime.
        CreateMap<Team, TeamDto>()
            .ForCtorParam(nameof(TeamDto.MemberCount), o => o.MapFrom(s => s.Members.Count));
        CreateMap<TeamMember, TeamMemberDto>()
            .ForCtorParam(nameof(TeamMemberDto.UserEmail), o => o.MapFrom(s => s.User != null ? s.User.Email : null))
            .ForCtorParam(nameof(TeamMemberDto.UserFullName), o => o.MapFrom(s => s.User != null ? s.User.FirstName + " " + s.User.LastName : null));
        CreateMap<Permission, PermissionDto>();
        CreateMap<Role, RoleDto>()
            .ForCtorParam(nameof(RoleDto.Permissions), o => o.MapFrom(s => s.RolePermissions.Select(rp => rp.Permission!.Key)));
        CreateMap<Invitation, InvitationDto>()
            .ForCtorParam(nameof(InvitationDto.RoleName), o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty));
        CreateMap<AuditLog, AuditLogDto>();

        // --- Phase 2: Projects / Milestones / Labels / WorkItems ---
        // Same rule as above applies everywhere below: any DTO member that isn't a straight 1:1
        // property copy (a count, a joined/denormalized name, a flattened child collection, a
        // computed aggregate like ActualHours) MUST use ForCtorParam, never ForMember.
        CreateMap<Project, ProjectDto>()
            .ForCtorParam(nameof(ProjectDto.MemberCount), o => o.MapFrom(s => s.Members.Count))
            .ForCtorParam(nameof(ProjectDto.WorkItemCounts), o => o.MapFrom(s =>
                s.WorkItems.GroupBy(w => w.Status).Select(g => new WorkItemStatusCountDto(g.Key, g.Count()))));
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForCtorParam(nameof(ProjectMemberDto.UserEmail), o => o.MapFrom(s => s.User != null ? s.User.Email : null))
            .ForCtorParam(nameof(ProjectMemberDto.UserFullName), o => o.MapFrom(s => s.User != null ? s.User.FirstName + " " + s.User.LastName : null));
        CreateMap<Milestone, MilestoneDto>();
        CreateMap<Label, LabelDto>();

        CreateMap<WorkItem, WorkItemDto>()
            .ForCtorParam(nameof(WorkItemDto.AssigneeFullName), o => o.MapFrom(s => s.AssigneeUser != null ? s.AssigneeUser.FirstName + " " + s.AssigneeUser.LastName : null))
            .ForCtorParam(nameof(WorkItemDto.ReporterFullName), o => o.MapFrom(s => s.ReporterUser != null ? s.ReporterUser.FirstName + " " + s.ReporterUser.LastName : null))
            .ForCtorParam(nameof(WorkItemDto.Labels), o => o.MapFrom(s => s.WorkItemLabels.Select(wl => wl.Label)));
        CreateMap<ChecklistItem, ChecklistItemDto>();
        CreateMap<WorkItemDependency, WorkItemDependencyDto>()
            .ForCtorParam(nameof(WorkItemDependencyDto.DependsOnTitle), o => o.MapFrom(s => s.DependsOnWorkItem != null ? s.DependsOnWorkItem.Title : string.Empty))
            .ForCtorParam(nameof(WorkItemDependencyDto.DependsOnStatus), o => o.MapFrom(s => s.DependsOnWorkItem != null ? s.DependsOnWorkItem.Status : Domain.WorkItemStatus.Backlog));
        CreateMap<WorkItemComment, WorkItemCommentDto>()
            .ForCtorParam(nameof(WorkItemCommentDto.AuthorFullName), o => o.MapFrom(s => s.AuthorUser != null ? s.AuthorUser.FirstName + " " + s.AuthorUser.LastName : null))
            .ForCtorParam(nameof(WorkItemCommentDto.MentionedUserIds), o => o.MapFrom(s => s.Mentions.Select(m => m.MentionedUserId)));
        CreateMap<WorkItemAttachment, WorkItemAttachmentDto>()
            .ForCtorParam(nameof(WorkItemAttachmentDto.DownloadUrl), o => o.MapFrom(s => $"/api/attachments/{s.Id}/download"));
        CreateMap<WorkItemActivity, WorkItemActivityDto>();
        CreateMap<WorkItemTimeLog, WorkItemTimeLogDto>();
        CreateMap<CustomFieldDefinition, CustomFieldDefinitionDto>();
        CreateMap<CustomFieldValue, CustomFieldValueDto>();

        CreateMap<WorkItem, WorkItemDetailDto>()
            .ForCtorParam(nameof(WorkItemDetailDto.AssigneeFullName), o => o.MapFrom(s => s.AssigneeUser != null ? s.AssigneeUser.FirstName + " " + s.AssigneeUser.LastName : null))
            .ForCtorParam(nameof(WorkItemDetailDto.ReporterFullName), o => o.MapFrom(s => s.ReporterUser != null ? s.ReporterUser.FirstName + " " + s.ReporterUser.LastName : null))
            .ForCtorParam(nameof(WorkItemDetailDto.Labels), o => o.MapFrom(s => s.WorkItemLabels.Select(wl => wl.Label)))
            .ForCtorParam(nameof(WorkItemDetailDto.ChecklistItems), o => o.MapFrom(s => s.ChecklistItems.OrderBy(c => c.Position)))
            .ForCtorParam(nameof(WorkItemDetailDto.FollowerUserIds), o => o.MapFrom(s => s.Followers.Select(f => f.UserId)))
            .ForCtorParam(nameof(WorkItemDetailDto.Dependencies), o => o.MapFrom(s => s.Dependencies))
            .ForCtorParam(nameof(WorkItemDetailDto.Comments), o => o.MapFrom(s => s.Comments.OrderBy(c => c.CreatedAt)))
            .ForCtorParam(nameof(WorkItemDetailDto.Attachments), o => o.MapFrom(s => s.Attachments))
            .ForCtorParam(nameof(WorkItemDetailDto.Activities), o => o.MapFrom(s => s.Activities.OrderByDescending(a => a.CreatedAt)))
            .ForCtorParam(nameof(WorkItemDetailDto.TimeLogs), o => o.MapFrom(s => s.TimeLogs.OrderByDescending(t => t.LoggedDate)))
            .ForCtorParam(nameof(WorkItemDetailDto.ActualHours), o => o.MapFrom(s => s.TimeLogs.Sum(t => t.Minutes) / 60m))
            .ForCtorParam(nameof(WorkItemDetailDto.CustomFieldValues), o => o.MapFrom(s => s.CustomFieldValues));

        // --- Phase 3: Scrum / Gantt / Calendar / Time Tracking ---
        // Same rule as above: every computed/projected member (counts, sums, joined names) goes
        // through ForCtorParam, never ForMember, since every DTO here is a positional record.
        CreateMap<Sprint, SprintDto>()
            .ForCtorParam(nameof(SprintDto.WorkItemCount), o => o.MapFrom(s => s.WorkItems.Count))
            .ForCtorParam(nameof(SprintDto.CompletedWorkItemCount), o => o.MapFrom(s => s.WorkItems.Count(w => w.Status == Domain.WorkItemStatus.Done)))
            .ForCtorParam(nameof(SprintDto.TotalStoryPoints), o => o.MapFrom(s => s.WorkItems.Sum(w => w.StoryPoints ?? 0)))
            .ForCtorParam(nameof(SprintDto.CompletedStoryPoints), o => o.MapFrom(s => s.WorkItems.Where(w => w.Status == Domain.WorkItemStatus.Done).Sum(w => w.StoryPoints ?? 0)));

        CreateMap<RetrospectiveNote, RetrospectiveNoteDto>()
            .ForCtorParam(nameof(RetrospectiveNoteDto.CreatedByName), o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FirstName + " " + s.CreatedByUser.LastName : null));

        CreateMap<GanttBaseline, GanttBaselineDto>()
            .ForCtorParam(nameof(GanttBaselineDto.ItemCount), o => o.MapFrom(s => s.Items.Count));

        CreateMap<GanttBaselineItem, GanttBaselineItemDto>();

        CreateMap<ActiveTimer, ActiveTimerDto>()
            .ForCtorParam(nameof(ActiveTimerDto.WorkItemTitle), o => o.MapFrom(s => s.WorkItem != null ? s.WorkItem.Title : string.Empty));

        // --- Phase 4: Chat / Notifications / Documents / Wiki ---
        // Same rule as every phase above: computed/projected members (joined names, grouped
        // reactions, counts, download URLs) go through ForCtorParam, never ForMember.
        CreateMap<MessageAttachment, MessageAttachmentDto>()
            .ForCtorParam(nameof(MessageAttachmentDto.DownloadUrl), o => o.MapFrom(s => $"/api/chat/attachments/{s.Id}/download"));

        CreateMap<ChatMessage, ChatMessageDto>()
            .ForCtorParam(nameof(ChatMessageDto.AuthorName), o => o.MapFrom(s => s.AuthorUser != null ? s.AuthorUser.FirstName + " " + s.AuthorUser.LastName : null))
            .ForCtorParam(nameof(ChatMessageDto.Reactions), o => o.MapFrom(s =>
                s.Reactions.GroupBy(r => r.Emoji).Select(g => new MessageReactionGroupDto(g.Key, g.Select(x => x.UserId).ToList()))))
            .ForCtorParam(nameof(ChatMessageDto.Attachments), o => o.MapFrom(s => s.Attachments))
            .ForCtorParam(nameof(ChatMessageDto.ReplyCount), o => o.MapFrom(s => s.Replies.Count));

        CreateMap<Notification, NotificationDto>();

        CreateMap<DocPageComment, DocPageCommentDto>()
            .ForCtorParam(nameof(DocPageCommentDto.AuthorName), o => o.MapFrom(s => s.AuthorUser != null ? s.AuthorUser.FirstName + " " + s.AuthorUser.LastName : null));

        CreateMap<DocPage, DocPageSummaryDto>()
            .ForCtorParam(nameof(DocPageSummaryDto.UpdatedByName), o => o.MapFrom(s =>
                s.UpdatedByUser != null ? s.UpdatedByUser.FirstName + " " + s.UpdatedByUser.LastName
                : s.CreatedByUser != null ? s.CreatedByUser.FirstName + " " + s.CreatedByUser.LastName : null))
            .ForCtorParam(nameof(DocPageSummaryDto.UpdatedAt), o => o.MapFrom(s => s.UpdatedAt ?? s.CreatedAt));

        CreateMap<DocPage, DocPageDetailDto>()
            .ForCtorParam(nameof(DocPageDetailDto.CreatedByName), o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FirstName + " " + s.CreatedByUser.LastName : null))
            .ForCtorParam(nameof(DocPageDetailDto.UpdatedByName), o => o.MapFrom(s => s.UpdatedByUser != null ? s.UpdatedByUser.FirstName + " " + s.UpdatedByUser.LastName : null))
            .ForCtorParam(nameof(DocPageDetailDto.VersionCount), o => o.MapFrom(s => s.Versions.Count));

        CreateMap<DocPageVersion, DocPageVersionSummaryDto>()
            .ForCtorParam(nameof(DocPageVersionSummaryDto.EditedByName), o => o.MapFrom(s => s.EditedByUser != null ? s.EditedByUser.FirstName + " " + s.EditedByUser.LastName : null));

        CreateMap<DocPageVersion, DocPageVersionDetailDto>()
            .ForCtorParam(nameof(DocPageVersionDetailDto.EditedByName), o => o.MapFrom(s => s.EditedByUser != null ? s.EditedByUser.FirstName + " " + s.EditedByUser.LastName : null));

        // --- Phase 6: AI Features / Workflow Automation ---
        // Same rule as every phase above: computed/projected members (counts, joined names, max
        // aggregates) go through ForCtorParam, never ForMember.
        CreateMap<AiChatMessage, AiChatMessageDto>();

        CreateMap<AiChatConversation, AiChatConversationSummaryDto>()
            .ForCtorParam(nameof(AiChatConversationSummaryDto.LastMessageAt), o => o.MapFrom(s =>
                s.Messages.Any() ? s.Messages.Max(m => m.CreatedAt) : s.CreatedAt));

        CreateMap<WorkflowDefinition, WorkflowSummaryDto>()
            .ForCtorParam(nameof(WorkflowSummaryDto.ConditionCount), o => o.MapFrom(s => s.Conditions.Count))
            .ForCtorParam(nameof(WorkflowSummaryDto.ActionCount), o => o.MapFrom(s => s.Actions.Count));

        CreateMap<WorkflowCondition, WorkflowConditionDto>();
        CreateMap<WorkflowAction, WorkflowActionDto>();

        CreateMap<WorkflowDefinition, WorkflowDetailDto>()
            .ForCtorParam(nameof(WorkflowDetailDto.Conditions), o => o.MapFrom(s => s.Conditions))
            .ForCtorParam(nameof(WorkflowDetailDto.Actions), o => o.MapFrom(s => s.Actions.OrderBy(a => a.Order)));

        CreateMap<WorkflowRun, WorkflowRunDto>();

        CreateMap<WorkflowApprovalRequest, ApprovalSummaryDto>()
            .ForCtorParam(nameof(ApprovalSummaryDto.WorkflowRunId), o => o.MapFrom(s => s.WorkflowRunId))
            .ForCtorParam(nameof(ApprovalSummaryDto.WorkflowName), o => o.MapFrom(s => s.WorkflowRun != null && s.WorkflowRun.WorkflowDefinition != null ? s.WorkflowRun.WorkflowDefinition.Name : string.Empty))
            .ForCtorParam(nameof(ApprovalSummaryDto.RequestedAt), o => o.MapFrom(s => s.CreatedAt));
    }
}
