using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Ai;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Policy = PermissionCatalog.AiUse)]
public class AiController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AiController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record GenerateTasksRequest(Guid ProjectId, string Prompt);
    public record PlanSprintRequest(Guid ProjectId, string SprintGoal, int CapacityPoints);
    public record AnalyzeBugRequest(Guid WorkItemId);
    public record SummarizeMeetingRequest(string RawNotes, Guid ProjectId, bool SaveAsWikiPage);
    public record ChatRequest(Guid? ProjectId, string Message, Guid? ConversationId);
    public record EstimateStoryPointsRequest(string Title, string? Description);
    public record PrioritizeTasksRequest(Guid ProjectId);
    public record ReviewCodeRequest(string Code, string Language);
    public record GenerateReleaseNotesRequest(Guid ProjectId, DateTime From, DateTime To, bool SaveAsWikiPage);

    [HttpPost("generate-tasks")]
    public async Task<ActionResult<GenerateTasksResultDto>> GenerateTasks(GenerateTasksRequest request)
        => Ok(await _mediator.Send(new GenerateTasksCommand(request.ProjectId, request.Prompt)));

    [HttpPost("plan-sprint")]
    public async Task<ActionResult<PlanSprintResultDto>> PlanSprint(PlanSprintRequest request)
        => Ok(await _mediator.Send(new PlanSprintCommand(request.ProjectId, request.SprintGoal, request.CapacityPoints)));

    [HttpPost("analyze-bug")]
    public async Task<ActionResult<AnalyzeBugResultDto>> AnalyzeBug(AnalyzeBugRequest request)
        => Ok(await _mediator.Send(new AnalyzeBugCommand(request.WorkItemId)));

    [HttpPost("summarize-meeting")]
    public async Task<ActionResult<SummarizeMeetingResultDto>> SummarizeMeeting(SummarizeMeetingRequest request)
        => Ok(await _mediator.Send(new SummarizeMeetingCommand(request.RawNotes, request.ProjectId, request.SaveAsWikiPage, CurrentUserId)));

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResultDto>> Chat(ChatRequest request)
        => Ok(await _mediator.Send(new ChatCommand(request.ProjectId, request.Message, request.ConversationId, CurrentUserId)));

    [HttpGet("chat/conversations")]
    public async Task<ActionResult<IReadOnlyList<AiChatConversationSummaryDto>>> ListConversations([FromQuery] Guid? projectId)
        => Ok(await _mediator.Send(new ListAiChatConversationsQuery(projectId, CurrentUserId)));

    [HttpGet("chat/conversations/{id:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<AiChatMessageDto>>> GetConversationMessages(Guid id)
        => Ok(await _mediator.Send(new GetAiChatMessagesQuery(id)));

    [HttpGet("risk-prediction")]
    public async Task<ActionResult<RiskPredictionDto>> RiskPrediction([FromQuery] Guid projectId)
        => Ok(await _mediator.Send(new GetRiskPredictionQuery(projectId)));

    [HttpGet("resource-allocation")]
    public async Task<ActionResult<ResourceAllocationResultDto>> ResourceAllocation([FromQuery] Guid projectId)
        => Ok(await _mediator.Send(new GetResourceAllocationQuery(projectId)));

    [HttpPost("estimate-story-points")]
    public async Task<ActionResult<EstimateStoryPointsResultDto>> EstimateStoryPoints(EstimateStoryPointsRequest request)
        => Ok(await _mediator.Send(new EstimateStoryPointsCommand(request.Title, request.Description)));

    [HttpGet("predict-deadline")]
    public async Task<ActionResult<PredictDeadlineResultDto>> PredictDeadline([FromQuery] Guid projectId)
        => Ok(await _mediator.Send(new GetPredictDeadlineQuery(projectId)));

    [HttpPost("prioritize-tasks")]
    public async Task<ActionResult<PrioritizeTasksResultDto>> PrioritizeTasks(PrioritizeTasksRequest request)
        => Ok(await _mediator.Send(new PrioritizeTasksCommand(request.ProjectId)));

    [HttpPost("review-code")]
    public async Task<ActionResult<ReviewCodeResultDto>> ReviewCode(ReviewCodeRequest request)
        => Ok(await _mediator.Send(new ReviewCodeCommand(request.Code, request.Language)));

    [HttpPost("generate-release-notes")]
    public async Task<ActionResult<GenerateReleaseNotesResultDto>> GenerateReleaseNotes(GenerateReleaseNotesRequest request)
        => Ok(await _mediator.Send(new GenerateReleaseNotesCommand(request.ProjectId, request.From, request.To, request.SaveAsWikiPage, CurrentUserId)));
}
