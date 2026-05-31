using Centeva.ObjectStorage.GCP;
using System.Net.Http;
using System.Net.Mime;

#pragma warning disable xUnit1051 // ReadAsStringAsync does not support CancellationToken on .NET Framework

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
    public async Task GetAsync_ContentType()
    {
        var storage = _fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions { ContentType = "application/json" };
        var path = await WriteToRandomPathAsync("", ".json", options);

        var entry = await storage.GetAsync(path, CancellationToken);

        entry.Should().NotBeNull();
        entry!.ContentType.Should().Be(options.ContentType);
    }

    [Fact]
    public async Task WriteAsync_WithContentDisposition_SetsHeaderWhenRetrieving()
    {
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var options = new WriteOptions
        {
            ContentType = "application/json",
            ContentDisposition = new ContentDisposition {FileName = "somefile.json"}
        };
        var path = await WriteToRandomPathAsync("", ".json", options);

        var signedUrl = await storage.GetDownloadUrlAsync(path, null, CancellationToken);

        using var client = new HttpClient();
        var response = await client.GetAsync(signedUrl, CancellationToken);
        response.EnsureSuccessStatusCode();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition.FileName.Should().Be(options.ContentDisposition.FileName);
        contentDisposition.DispositionType.Should().Be(options.ContentDisposition.DispositionType);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_ReturnsValidUrl()
    {
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var path = await WriteToRandomPathAsync("", ".json");
        var signedUrl = await storage.GetDownloadUrlAsync(path, options: null, CancellationToken);

        using var client = new HttpClient();
        var response = await client.GetAsync(signedUrl, CancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be(_testFileContent);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_WithContentDisposition_SetsHeaderWhenRetrieving()
    {
        var storage = (GoogleObjectStorage)_fixture.CreateStorage(TestSettings.Instance);
        var path = await WriteToRandomPathAsync("", ".json");
        var options = new SignedUrlOptions
        {
            ContentDisposition = new ContentDisposition { FileName = "somefile.json" }
        };
        var signedUrl = await storage.GetDownloadUrlAsync(path, options, CancellationToken);

        using var client = new HttpClient();
        var response = await client.GetAsync(signedUrl, CancellationToken);
        response.EnsureSuccessStatusCode();
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition.FileName.Should().Be(options.ContentDisposition.FileName);
        contentDisposition.DispositionType.Should().Be(options.ContentDisposition.DispositionType);
    }
}
