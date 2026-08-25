using Centeva.ObjectStorage.Builtin;
using Centeva.ObjectStorage.UnitTests.Fixtures;

using Microsoft.Extensions.DependencyInjection;

namespace Centeva.ObjectStorage.UnitTests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RegistersStorageFromConnectionString()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config
            .UseConnectionString("disk://path=/tmp"));

        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IObjectStorage>().Should().BeOfType<DiskObjectStorage>();
    }

    [Fact]
    public void DoesNotRegisterStorageFactory()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config.UseConnectionString("disk://path=/tmp"));

        var provider = services.BuildServiceProvider();

        provider.GetService<StorageFactory>().Should().BeNull();
    }

    [Fact]
    public void UsesProvidersRegisteredOnBuilder()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config =>
        {
            config.Register(new TestProviderFactory());
            config.UseConnectionString("test://param=one");
        });

        var storage = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        storage.Should().BeOfType<TestProvider>();
    }

    [Fact]
    public void RegistersProvidedStorageInstance()
    {
        var services = new ServiceCollection();
        var expected = new TestProvider();

        services.AddObjectStorage(config => config.UseStorage(expected));

        var storage = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        storage.Should().BeSameAs(expected);
    }

    [Fact]
    public void RegistersStorageAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config.UseConnectionString("disk://path=/tmp"));

        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IObjectStorage>()
            .Should().BeSameAs(provider.GetRequiredService<IObjectStorage>());
    }

    [Fact]
    public void ExposesServiceCollectionToBuilder()
    {
        var services = new ServiceCollection();
        IServiceCollection? captured = null;

        services.AddObjectStorage(config =>
        {
            captured = config.Services;
            config.UseConnectionString("disk://path=/tmp");
        });

        captured.Should().BeSameAs(services);
    }

    [Fact]
    public void LastConfigurationWins()
    {
        var services = new ServiceCollection();
        var storage = new TestProvider();

        services.AddObjectStorage(config => config
            .UseStorage(storage)
            .UseConnectionString("disk://path=/tmp"));

        var resolved = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        resolved.Should().BeOfType<DiskObjectStorage>();
    }

    [Fact]
    public void ThrowsWhenNoStorageConfigured()
    {
        var services = new ServiceCollection();

        var act = () => services.AddObjectStorage(_ => { });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UseConnectionString*");
    }

    [Fact]
    public void ThrowsWhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddObjectStorage(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegistersKeyedStorage()
    {
        var services = new ServiceCollection();

        services.AddKeyedObjectStorage("disk", config => config.UseConnectionString("disk://path=/tmp"));

        var storage = services.BuildServiceProvider().GetRequiredKeyedService<IObjectStorage>("disk");

        storage.Should().BeOfType<DiskObjectStorage>();
    }

    [Fact]
    public void RegistersMultipleKeyedStorages()
    {
        var services = new ServiceCollection();
        var testStorage = new TestProvider();

        services.AddKeyedObjectStorage("disk", config => config.UseConnectionString("disk://path=/tmp"));
        services.AddKeyedObjectStorage("test", config => config.UseStorage(testStorage));

        var provider = services.BuildServiceProvider();

        provider.GetRequiredKeyedService<IObjectStorage>("disk").Should().BeOfType<DiskObjectStorage>();
        provider.GetRequiredKeyedService<IObjectStorage>("test").Should().BeSameAs(testStorage);
    }

    [Fact]
    public void KeyedStorageIsNotResolvableWithoutKey()
    {
        var services = new ServiceCollection();

        services.AddKeyedObjectStorage("disk", config => config.UseConnectionString("disk://path=/tmp"));

        var provider = services.BuildServiceProvider();

        provider.GetService<IObjectStorage>().Should().BeNull();
    }

    [Fact]
    public void KeyedRegistrationThrowsWhenNoStorageConfigured()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKeyedObjectStorage("disk", _ => { });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*UseConnectionString*");
    }

    [Fact]
    public void KeyedRegistrationThrowsWhenConfigureIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddKeyedObjectStorage("disk", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegistersSupportedCapabilityInterfaces()
    {
        var services = new ServiceCollection();
        var storage = new TestMetadataProvider();

        services.AddObjectStorage(config => config.UseStorage(storage));

        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<ISupportsSignedUrls>().Should().BeSameAs(storage);
        provider.GetRequiredService<ISupportsMetadata>().Should().BeSameAs(storage);
    }

    [Fact]
    public void DoesNotRegisterUnsupportedCapabilityInterfaces()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config.UseConnectionString("disk://path=/tmp"));

        var provider = services.BuildServiceProvider();

        provider.GetService<ISupportsSignedUrls>().Should().BeNull();
        provider.GetService<ISupportsMetadata>().Should().BeNull();
    }

    [Fact]
    public void RegistersOnlyTheCapabilitiesTheProviderImplements()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config.UseStorage(new TestProvider()));

        var provider = services.BuildServiceProvider();

        provider.GetService<ISupportsSignedUrls>().Should().NotBeNull();
        provider.GetService<ISupportsMetadata>().Should().BeNull();
    }

    [Fact]
    public void RegistersKeyedCapabilityInterfaces()
    {
        var services = new ServiceCollection();
        var storage = new TestMetadataProvider();

        services.AddKeyedObjectStorage("test", config => config.UseStorage(storage));

        var provider = services.BuildServiceProvider();

        provider.GetRequiredKeyedService<ISupportsSignedUrls>("test").Should().BeSameAs(storage);
        provider.GetRequiredKeyedService<ISupportsMetadata>("test").Should().BeSameAs(storage);
        provider.GetService<ISupportsSignedUrls>().Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ThrowsWhenConnectionStringIsMissing(string? connectionString)
    {
        var services = new ServiceCollection();

        var act = () => services.AddObjectStorage(config => config.UseConnectionString(connectionString!));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*configuration*");
    }

    [Fact]
    public void RegistersObjectStorageFactory()
    {
        var services = new ServiceCollection();

        services.AddObjectStorageFactory(config => config.Register(new TestProviderFactory()));

        var factory = services.BuildServiceProvider().GetRequiredService<IObjectStorageFactory>();

        var storage = factory.CreateConnection("test://param=one");

        storage.Should().BeOfType<TestProvider>();
    }

    [Fact]
    public void ObjectStorageFactoryResolvesDifferentConnectionsAtRuntime()
    {
        var services = new ServiceCollection();

        services.AddObjectStorageFactory(config => config.Register(new TestProviderFactory()));

        var factory = services.BuildServiceProvider().GetRequiredService<IObjectStorageFactory>();

        var disk = factory.CreateConnection("disk://path=/tmp");
        var test = factory.CreateConnection("test://param=one");

        disk.Should().BeOfType<DiskObjectStorage>();
        test.Should().BeOfType<TestProvider>();
    }

    [Fact]
    public void RegistersObjectStorageFactoryAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddObjectStorageFactory(config => config.Register(new TestProviderFactory()));

        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IObjectStorageFactory>()
            .Should().BeSameAs(provider.GetRequiredService<IObjectStorageFactory>());
    }

    [Fact]
    public void ObjectStorageFactoryThrowsWithUnrecognizedProvider()
    {
        var services = new ServiceCollection();

        services.AddObjectStorageFactory(config => { });

        var factory = services.BuildServiceProvider().GetRequiredService<IObjectStorageFactory>();

        var act = () => factory.CreateConnection("test://param=one");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ThrowsWhenObjectStorageFactoryConfigureIsNull()
    {
        var services = new ServiceCollection();

        var act = () => services.AddObjectStorageFactory(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
