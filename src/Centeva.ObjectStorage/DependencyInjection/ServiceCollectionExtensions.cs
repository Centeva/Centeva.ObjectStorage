using Centeva.ObjectStorage;
using Centeva.ObjectStorage.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ObjectStorageServiceCollectionExtensions
{
    /// <summary>
    /// Register an <see cref="IObjectStorage"/> instance as a singleton.  The
    /// optional capability interfaces implemented by the configured provider,
    /// such as <see cref="ISupportsSignedUrls"/> and
    /// <see cref="ISupportsMetadata"/>, are registered against the same
    /// instance so they can be injected directly.
    /// </summary>
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, Action<ObjectStorageBuilder> configure)
    {
        var storage = BuildStorage(services, configure);

        services.AddSingleton(storage);

        if (storage is ISupportsSignedUrls signedUrls)
        {
            services.AddSingleton(signedUrls);
        }

        if (storage is ISupportsMetadata metadata)
        {
            services.AddSingleton(metadata);
        }

        return services;
    }

    /// <summary>
    /// Register an <see cref="IObjectStorage"/> instance as a keyed singleton,
    /// allowing multiple storage configurations to coexist.  Resolve it using
    /// <c>[FromKeyedServices(key)]</c> or
    /// <c>GetRequiredKeyedService&lt;IObjectStorage&gt;(key)</c>.  The optional
    /// capability interfaces implemented by the configured provider, such as
    /// <see cref="ISupportsSignedUrls"/> and <see cref="ISupportsMetadata"/>,
    /// are registered against the same instance and key.
    /// </summary>
    public static IServiceCollection AddKeyedObjectStorage(this IServiceCollection services, object? serviceKey, Action<ObjectStorageBuilder> configure)
    {
        var storage = BuildStorage(services, configure);

        services.AddKeyedSingleton(serviceKey, storage);

        if (storage is ISupportsSignedUrls signedUrls)
        {
            services.AddKeyedSingleton(serviceKey, signedUrls);
        }

        if (storage is ISupportsMetadata metadata)
        {
            services.AddKeyedSingleton(serviceKey, metadata);
        }

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
