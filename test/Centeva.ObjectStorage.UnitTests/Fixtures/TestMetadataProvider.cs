namespace Centeva.ObjectStorage.UnitTests.Fixtures;

/// <summary>
/// Test provider that supports metadata in addition to the capabilities
/// provided by <see cref="TestProvider"/>.
/// </summary>
internal class TestMetadataProvider : TestProvider, ISupportsMetadata
{
    public Task UpdateMetadataAsync(StoragePath path, UpdateStorageEntryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
