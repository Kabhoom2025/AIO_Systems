using Polly;
using Polly.Retry;

namespace FlowSphere.Execution.Retry;

public static class RetryPolicyFactory
{
    /// <summary>Default per-node policy: 3 attempts, exponential backoff. Retries on any
    /// exception thrown by the node executor - node executors signal expected failures via
    /// NodeResult.Fail (not an exception), so this only catches genuinely unexpected faults.</summary>
    public static AsyncRetryPolicy Create(int maxAttempts = 3)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: maxAttempts - 1,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)));
    }
}
