using Centeva.ObjectStorage.Azure.Blob;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class AzureBlobObjectStorageFixture : ObjectStorageFixture
{
    public AzureBlobObjectStorageFixture() : base("/" + nameof(AzureBlobObjectStorageTests) + "/")
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return new AzureBlobObjectStorage(settings.AzureAccountName!, settings.AzureAccountKey!, settings.AzureBlobStorageTestContainerName!);
    }

    public override void Cleanup()
    {
    }
}

public class AzureBlobObjectStorageTests : CommonObjectStorageTests, IClassFixture<AzureBlobObjectStorageFixture>
{
    public AzureBlobObjectStorageTests(AzureBlobObjectStorageFixture fixture) : base(fixture)
    {
    }
}
