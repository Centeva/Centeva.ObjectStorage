namespace Centeva.ObjectStorage.IntegrationTests;

public abstract class ObjectStorageFixture : IDisposable
{
    private static readonly TestSettings _settings = TestSettings.Instance;

    /// <summary>
    /// Prefix to use for all object names.  Useful for running tests in a cloud storage bucket that
    /// already has other things in it, to avoid collisions.
    /// </summary>
    public string? StoragePathPrefix { get;}

    public IObjectStorage Storage { get; }

    public ObjectStorageFixture(string? storagePathPrefix = null)
    {
        Storage = CreateStorage(_settings);
        StoragePathPrefix = storagePathPrefix;
    }

    public abstract IObjectStorage CreateStorage(TestSettings settings);

    public abstract void Cleanup();

    public void Dispose()
    {
        Cleanup();
        GC.SuppressFinalize(this);
    }
}