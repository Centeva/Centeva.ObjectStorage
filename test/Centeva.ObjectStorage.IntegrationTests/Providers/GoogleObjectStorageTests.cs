using Centeva.ObjectStorage.GCP;

namespace Centeva.ObjectStorage.IntegrationTests.Providers;

public class GoogleObjectStorageFixture : ObjectStorageFixture
{
    public GoogleObjectStorageFixture() : base("/" + nameof(GoogleObjectStorageTests) + "/")
    {
    }

    public override IObjectStorage CreateStorage(TestSettings settings)
    {
        return GoogleObjectStorage.CreateFromCredentialsJson(settings.GoogleStorageTestBucketName!, settings.GoogleStorageCredentialsJson!);
    }

    public override void Cleanup()
    {
    }
}

public class GoogleObjectStorageTests : CommonObjectStorageTests, IClassFixture<GoogleObjectStorageFixture>
{
    private readonly GoogleObjectStorageFixture _fixture;
    public GoogleObjectStorageTests(GoogleObjectStorageFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }


    [Fact]
    public async void GetAsync_ContentType()
    {
        var storage = _fixture.CreateStorage(TestSettings.Instance);
        var contentType = "application/json";
        var path = await WriteToRandomPathAsync("", ".json", contentType);

        var entry = await storage.GetAsync(path);

        entry.Should().NotBeNull();
        entry!.ContentType.Should().Be(contentType);
    }
}
