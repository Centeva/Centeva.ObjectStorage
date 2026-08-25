using Centeva.ObjectStorage;
using Centeva.ObjectStorage.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ObjectStorageServiceCollectionExtensions
{
    /// <summary>
    /// Register an <see cref="IObjectStorage"/> instance as a singleton.
    /// </summary>
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, Action<ObjectStorageBuilder> configure)
    {
        services.AddSingleton(BuildStorage(services, configure));

        return services;
    }

    /// <summary>
    /// Register an <see cref="IObjectStorage"/> instance as a keyed singleton,
    /// allowing multiple storage configurations to coexist.  Resolve it using
    /// <c>[FromKeyedServices(key)]</c> or
    /// <c>GetRequiredKeyedService&lt;IObjectStorage&gt;(key)</c>.
    /// </summary>
    public static IServiceCollection AddKeyedObjectStorage(this IServiceCollection services, object? serviceKey, Action<ObjectStorageBuilder> configure)
    {
        services.AddKeyedSingleton(serviceKey, BuildStorage(services, configure));

        return services;
    }

    private static IObjectStorage BuildStorage(IServiceCollection services, Action<ObjectStorageBuilder> configure)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var builder = new ObjectStorageBuilder(services);
        configure(builder);

        return builder.Build();
    }
}
