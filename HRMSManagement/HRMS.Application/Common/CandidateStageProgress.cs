namespace HRMS.Application.Common;

/// <summary>Shared stage-order/progress logic used by both the HR recruitment view and the public candidate tracking page.</summary>
public static class CandidateStageProgress
{
    public static readonly string[] Stages = { "Applied", "Screening", "Interview", "Offered", "Hired" };

    /// <summary>0-100. Rejected candidates keep the percent of the last stage they reached before rejection.</summary>
    public static int GetProgressPercent(string currentStage, IEnumerable<string> historyStages)
    {
        if (currentStage != "Rejected")
        {
            var index = Array.IndexOf(Stages, currentStage);
            return index < 0 ? 0 : (int)Math.Round((index + 1) * 100.0 / Stages.Length);
        }

        // Rejected: use the furthest non-rejected stage reached in history.
        var reachedIndex = historyStages
            .Select(s => Array.IndexOf(Stages, s))
            .Where(i => i >= 0)
            .DefaultIfEmpty(-1)
            .Max();

        return reachedIndex < 0 ? 0 : (int)Math.Round((reachedIndex + 1) * 100.0 / Stages.Length);
    }
}
