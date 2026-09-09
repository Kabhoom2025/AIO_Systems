using Microsoft.Extensions.DependencyInjection;

namespace FlowSphere.Tests.TestUtilities;

/// <summary>Minimal IServiceScopeFactory for engine tests that exercise Parallel branches - each
/// CreateScope() call invokes the supplied factory to build a fresh set of scoped services
/// (typically a new FlowSphereDbContext instance pointed at the same InMemory database name),
/// mirroring how the real DI container hands each branch its own DbContext.</summary>
public class FakeServiceScopeFactory : IServiceScopeFactory
{
    private readonly Func<IServiceProvider> _providerFactory;

    public FakeServiceScopeFactory(Func<IServiceProvider> providerFactory)
    {
        _providerFactory = providerFactory;
    }

    public IServiceScope CreateScope() => new FakeServiceScope(_providerFactory());
}

public class FakeServiceScope : IServiceScope
{
    public IServiceProvider ServiceProvider { get; }

    public FakeServiceScope(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

public class FakeServiceProvider : IServiceProvider, IDisposable
{
    private readonly Dictionary<Type, object> _services;

    public FakeServiceProvider(Dictionary<Type, object> services)
    {
        _services = services;
    }

    public object? GetService(Type serviceType) => _services.TryGetValue(serviceType, out var service) ? service : null;

    public void Dispose()
    {
        foreach (var service in _services.Values)
        {
            (service as IDisposable)?.Dispose();
        }
    }
}
