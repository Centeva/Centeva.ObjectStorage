using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace Centeva.ObjectStorage.Azure;

public class AzureObjectStorage : ISignedUrlObjectStorage
{
    private readonly BlobServiceClient _client;
    private readonly AzureObjectStorageConnectionString _connectionString;
    private readonly string _containerName;

    public AzureObjectStorage(string containerName, AzureObjectStorageConnectionString? connectionString, string accountName, string accountKey)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));

        _containerName = containerName;
        _connectionString = connectionString;

        if (_connectionString.GetRequired("AccountName") != accountName)
            throw new ArgumentException("AccountName mismatch", nameof(accountName));

        if (_connectionString.GetRequired("AccountKey") != accountKey)
            throw new ArgumentException("AccountKey mismatch", nameof(accountKey));

        // The connection string contains the parts that make up the URI endpoint
        // {DefaultEndpointsProtocol}://{AccountName}.blob.{EndpointSuffix}
        var protocol = _connectionString.Get("DefaultEndpointsProtocol") ?? "https";
        var suffix = _connectionString.Get("EndpointSuffix") ?? "core.windows.net";
        var endpoint = $"{protocol}://{accountName}.blob.{suffix}";

        StorageSharedKeyCredential credentials = new(accountName, accountKey);        
        Uri uri = new(endpoint);
        _client = new BlobServiceClient(uri, credentials);
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

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
            .GetBlobContainerClient(_containerName)
            .GetBlobs(cancellationToken: cancellationToken)
            .Select(x => x.Name);
        return Task.FromResult(results);
    }

    public async Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        return await _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(objectName)
            .OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task WriteAsync(string objectName, Stream dataStream, string? contentType = null, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        await _client
            .GetBlobContainerClient(_containerName)
            .CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        await _client
            .GetBlobContainerClient(_containerName)
            .UploadBlobAsync(objectName, dataStream, cancellationToken)
            .ConfigureAwait(false);
    }
}
