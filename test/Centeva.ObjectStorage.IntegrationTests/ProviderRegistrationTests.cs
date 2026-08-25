using Centeva.ObjectStorage.AWS;
using Centeva.ObjectStorage.Azure.Blob;
using Centeva.ObjectStorage.Azure.File;
using Centeva.ObjectStorage.GCP;

using Microsoft.Extensions.DependencyInjection;

namespace Centeva.ObjectStorage.IntegrationTests;

/// <summary>
/// Each provider package's <c>StorageFactoryExtensions</c> (e.g.
/// <c>UseAwsS3Storage()</c>) are documented to chain directly into
/// <c>ObjectStorageBuilder.UseConnectionString</c>, as shown in the README:
/// <code>
/// builder.Services.AddObjectStorage(config => config
///     .UseAwsS3Storage()
///     .UseConnectionString(...));
/// </code>
/// </summary>
public class ProviderRegistrationTests
{
    [Fact]
    public void UseAwsS3Storage_ChainsIntoUseConnectionString()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config
            .UseAwsS3Storage()
            .UseConnectionString("aws.s3://bucket=test-bucket;region=us-east-1;accessKey=test-access-key;secretKey=test-secret-key"));

        var storage = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        storage.Should().BeOfType<AwsS3ObjectStorage>();
    }

    [Fact]
    public void UseAzureBlobStorage_ChainsIntoUseConnectionString()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config
            .UseAzureBlobStorage()
            .UseConnectionString($"azure.blob://container=test-container;accountName=testaccount;accountKey={FakeAzureAccountKey}"));

        var storage = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        storage.Should().BeOfType<AzureBlobObjectStorage>();
    }

    [Fact]
    public void UseAzureFileStorage_ChainsIntoUseConnectionString()
    {
        var services = new ServiceCollection();

        services.AddObjectStorage(config => config
            .UseAzureFileStorage()
            .UseConnectionString($"azure.file://share=test-share;accountName=testaccount;accountKey={FakeAzureAccountKey}"));

        var storage = services.BuildServiceProvider().GetRequiredService<IObjectStorage>();

        storage.Should().BeOfType<AzureFileStorage>();
    }

    // StorageSharedKeyCredential requires valid base64, but the decoded
    // content is never validated -- compute it from an obviously-fake
    // plaintext, rather than a base64 literal, so nothing that looks like a
    // real account key appears in source.
    private static string FakeAzureAccountKey => Convert.ToBase64String("not-a-real-account-key"u8.ToArray());

    [Fact]
    public void UseGoogleCloudStorage_ChainsIntoUseConnectionString()
    {
        var services = new ServiceCollection();

        // GoogleObjectStorage parses its credentials JSON eagerly, so a fake
        // credential fails here regardless of chaining -- what this asserts
        // is that the connection string was routed to the Google provider at
        // all (rather than failing with "Could not find a storage provider",
        // which is what the chaining bug would produce if UseGoogleCloudStorage
        // silently failed to register).
        var fakeCredentials = Convert.ToBase64String("{\"type\":\"service_account\"}"u8.ToArray());

        var act = () => services.AddObjectStorage(config => config
            .UseGoogleCloudStorage()
            .UseConnectionString($"google.storage://bucket=test-bucket;credentials={fakeCredentials}"));

        act.Should().Throw<Exception>()
            .Which.Message.Should().NotContain("Could not find a storage provider");
    }
}
