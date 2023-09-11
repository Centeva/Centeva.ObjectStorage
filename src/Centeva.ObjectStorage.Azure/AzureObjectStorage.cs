using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.Azure;

public class AzureObjectStorage : ISignedUrlObjectStorage
{
    private readonly BlobServiceClient _client;
    private string? _containerName = null;

    public AzureObjectStorage(string accountName, string accountKey, Uri azureEndpoint)
    {
        StorageSharedKeyCredential credentials = new(accountName, accountKey);        
        _client = new BlobServiceClient(azureEndpoint, credentials);
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);
        _containerName ??= ExtractContainerName(ref objectName);

        await _client
            .GetBlobContainerClient(_containerName)
            .DeleteBlobAsync(objectName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var blobNames = (await ListAsync(0, cancellationToken)).Distinct().ToHashSet();
        return blobNames.Contains(objectName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Uri> GetDownloadUrlAsync(string objectName, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);
        _containerName ??= ExtractContainerName(ref objectName);

        var blobClient = _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(objectName);

        if (!blobClient.CanGenerateSasUri)
            return await Task.FromResult(new Uri(_client.Uri, $"{_containerName}/{objectName}"));

        BlobSasBuilder sasBuilder = new()
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(lifetimeInSeconds)
        };
        sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

        Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri;
    }

    public Task<IEnumerable<string>> ListAsync(int pageSize, CancellationToken cancellationToken = default)
    {
        var results = _client
            .GetBlobContainerClient(_containerName ?? "$root")
            .GetBlobs(cancellationToken: cancellationToken)
            .Select(x => x.Name);
        return Task.FromResult(results);
    }

    public async Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);
        _containerName ??= ExtractContainerName(ref objectName);

        return await _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(objectName)
            .OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task WriteAsync(string objectName, Stream dataStream, string? contentType = null, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);
        _containerName ??= ExtractContainerName(ref objectName);

        await _client
            .GetBlobContainerClient(_containerName)
            .CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        await _client
            .GetBlobContainerClient(_containerName)
            .UploadBlobAsync(objectName, dataStream, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string ExtractContainerName(ref string objectName)
    {
        if (objectName.Length == 0)
            return objectName;

        var index = objectName.IndexOf('/');
        if (index == -1)
            return "$root";

        var containerName = objectName[..index];
        objectName = objectName[index..];
        return containerName;
    }
}
