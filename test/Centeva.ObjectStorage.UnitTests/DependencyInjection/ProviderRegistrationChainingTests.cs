using Centeva.ObjectStorage.DependencyInjection;
using Centeva.ObjectStorage.UnitTests.Fixtures;

using Microsoft.Extensions.DependencyInjection;

namespace Centeva.ObjectStorage.UnitTests.DependencyInjection;

/// <summary>
/// A provider package's <c>StorageFactoryExtensions</c> (e.g.
/// <c>Centeva.ObjectStorage.AWS</c>'s <c>UseAwsS3Storage</c>) are documented
/// to chain directly into <see cref="ObjectStorageBuilder.UseConnectionString"/>,
/// as shown in the README:
/// <code>
/// builder.Services.AddObjectStorage(config => config
///     .UseAwsS3Storage()
///     .UseConnectionString(...));
/// </code>
/// <see cref="TestProviderFactoryExtensions.UseTestProvider"/> mirrors that
/// shape without requiring a real provider package.
/// </summary>
public class ProviderRegistrationChainingTests
{
    [Fact]
    public void ProviderRegistrationExtensionChainsIntoUseConnectionString()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config
            .UseTestProvider()
            .UseConnectionString("test://param=one"));

        var storage = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        storage.Should().BeOfType<TestProvider>();
    }
}
