namespace Centeva.ObjectStorage.UnitTests.Fixtures;

/// <summary>
/// Mirrors the shape of a real provider package's <c>StorageFactoryExtensions</c>
/// (e.g. <c>Centeva.ObjectStorage.AWS</c>'s <c>UseAwsS3Storage</c>) so the
/// fluent DI chaining behavior they rely on can be exercised without taking a
/// dependency on an actual provider package.
/// </summary>
internal static class TestProviderFactoryExtensions
{
    public static TRegistry UseTestProvider<TRegistry>(this TRegistry registry) where TRegistry : IObjectStorageProviderRegistry
    {
        registry.Register(new TestProviderFactory());

        return registry;
    }
}
