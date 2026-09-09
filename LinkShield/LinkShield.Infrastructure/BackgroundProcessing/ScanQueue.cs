using System.Threading.Channels;
using LinkShield.Application.Interfaces;

namespace LinkShield.Infrastructure.BackgroundProcessing;

public class ScanQueue : IScanQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });

    public void Enqueue(Guid scanId) => _channel.Writer.TryWrite(scanId);

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}
