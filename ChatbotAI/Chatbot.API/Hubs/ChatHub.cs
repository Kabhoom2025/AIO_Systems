using System.Collections.Concurrent;
using System.Security.Claims;
using Chatbot.Application.Chat;
using Chatbot.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chatbot.API.Hubs;

[Authorize]
public class ChatHub(
    IChatOrchestrationService orchestrationService,
    ILogger<ChatHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> ActiveGenerations = new();

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        CancelActiveGeneration(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid? conversationId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            await Clients.Caller.SendAsync("Error", new { message = "Message cannot be empty.", errorCode = "VALIDATION_ERROR" });
            return;
        }

        var userId = GetUserId();
        var cts = RegisterCancellation(Context.ConnectionId);

        try
        {
            var prepared = await orchestrationService.PrepareTurnAsync(userId, conversationId, message, cts.Token);
            await Clients.Caller.SendAsync("MessageStarted", new { prepared.ConversationId, userMessage = prepared.UserMessage });
            await Clients.Caller.SendAsync("TypingStarted", new { prepared.ConversationId });

            await StreamAndPersistAsync(userId, prepared, cts.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Generation cancelled for connection {ConnectionId}", Context.ConnectionId);
        }
        catch (AiServiceException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message, errorCode = ex.ErrorCode });
        }
        catch (NotFoundException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message, errorCode = "NOT_FOUND" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error generating a response for connection {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { message = "Unable to connect to the AI service. Please try again.", errorCode = "AI_SERVICE_ERROR" });
        }
        finally
        {
            ActiveGenerations.TryRemove(Context.ConnectionId, out _);
        }
    }

    public async Task RegenerateMessage(Guid assistantMessageId)
    {
        var userId = GetUserId();
        var cts = RegisterCancellation(Context.ConnectionId);

        try
        {
            var prepared = await orchestrationService.PrepareRegenerateAsync(userId, assistantMessageId, cts.Token);
            await Clients.Caller.SendAsync("MessageStarted", new { prepared.ConversationId, userMessage = (object?)null, regeneratedMessageId = assistantMessageId });
            await Clients.Caller.SendAsync("TypingStarted", new { prepared.ConversationId });

            await StreamAndPersistAsync(userId, prepared, cts.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Regeneration cancelled for connection {ConnectionId}", Context.ConnectionId);
        }
        catch (AiServiceException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message, errorCode = ex.ErrorCode });
        }
        catch (NotFoundException ex)
        {
            await Clients.Caller.SendAsync("Error", new { message = ex.Message, errorCode = "NOT_FOUND" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error generating a response for connection {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", new { message = "Unable to connect to the AI service. Please try again.", errorCode = "AI_SERVICE_ERROR" });
        }
        finally
        {
            ActiveGenerations.TryRemove(Context.ConnectionId, out _);
        }
    }

    public Task StopGeneration()
    {
        CancelActiveGeneration(Context.ConnectionId);
        return Task.CompletedTask;
    }

    private async Task StreamAndPersistAsync(Guid userId, PreparedTurn prepared, CancellationToken cancellationToken)
    {
        var contentBuilder = new System.Text.StringBuilder();
        string model = string.Empty;
        int? promptTokens = null;
        int? completionTokens = null;

        var aiService = await orchestrationService.ResolveAiServiceAsync(userId, cancellationToken);

        await foreach (var chunk in aiService.StreamResponseAsync("", prepared.History, cancellationToken))
        {
            if (!chunk.IsFinal && !string.IsNullOrEmpty(chunk.DeltaContent))
            {
                contentBuilder.Append(chunk.DeltaContent);
                await Clients.Caller.SendAsync("MessageChunk", new { prepared.ConversationId, delta = chunk.DeltaContent }, cancellationToken);
            }

            if (chunk.IsFinal)
            {
                model = chunk.Model ?? model;
                promptTokens = chunk.PromptTokens;
                completionTokens = chunk.CompletionTokens;
            }
        }

        await Clients.Caller.SendAsync("TypingStopped", new { prepared.ConversationId }, cancellationToken);

        var assistantMessage = await orchestrationService.CompleteAssistantMessageAsync(
            prepared.ConversationId, contentBuilder.ToString(), model, promptTokens, completionTokens, cancellationToken);

        await Clients.Caller.SendAsync("MessageCompleted", new { prepared.ConversationId, message = assistantMessage }, cancellationToken);
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAppException("Invalid user context.");
    }

    private static CancellationTokenSource RegisterCancellation(string connectionId)
    {
        CancelActiveGeneration(connectionId);
        var cts = new CancellationTokenSource();
        ActiveGenerations[connectionId] = cts;
        return cts;
    }

    private static void CancelActiveGeneration(string connectionId)
    {
        if (ActiveGenerations.TryRemove(connectionId, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }
    }
}
