namespace FlowSphere.Domain.Enums;

public enum WorkflowNodeType
{
    Trigger,
    HttpRequest,
    Condition,
    Delay,
    EmailSmtp,
    AiPrompt,
    SlackMessage,
    TeamsMessage,
    DiscordMessage,
    Decision,
    Loop,
    Parallel,
    Merge,
    Exception,
    UserTask,
    Webhook,
    Twilio,
    Notion,
    Jira,
    Airtable,
    GoogleSheets,

    /// <summary>Explicit end-of-flow marker (Quixy calls this "Terminate") - when the engine
    /// reaches this node it completes the execution immediately, instead of relying on the
    /// implicit "ran off the graph, no more edges" fallback every other node type uses.</summary>
    Terminate
}
