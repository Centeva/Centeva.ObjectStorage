using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace Centeva.ObjectStorage.Azure.Blob;

public class AzureBlobObjectStorage : ISignedUrlObjectStorage
{
    private readonly BlobServiceClient _client;
    private readonly string? _containerName = null;

    public AzureBlobObjectStorage(string accountName, string accountKey, string container, Uri? serviceUri = null)
    {
        _containerName = container;
        StorageSharedKeyCredential credentials = new(accountName, accountKey);
        _client = new BlobServiceClient(serviceUri ?? GetServiceUri(accountName), credentials);
    }

    public Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var files = new List<string>();
        var results = _client
            .GetBlobContainerClient(_containerName)
            .GetBlobs(cancellationToken: cancellationToken);

        files.AddRange(results.Select(x => StoragePath.Normalize(x.Name)));

        return Task.FromResult<IReadOnlyCollection<string>>(files);
    }

    public async Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var blobClient = _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(path.WithoutLeadingSlash);

        try
        {
            await blobClient
                .GetPropertiesAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            return false;
        }
    }

    public async Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var blobClient = _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(path.WithoutLeadingSlash);

        try
        {
            var properties = await blobClient
                .GetPropertiesAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new StorageEntry(path)
            {
                CreationTime = properties.Value.CreatedOn,
                LastModificationTime = properties.Value.LastModified,
                SizeInBytes = properties.Value.ContentLength
            };
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            return null;
        }
    }

    public async Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client
                .GetBlobContainerClient(_containerName)
                .GetBlobClient(path.WithoutLeadingSlash)
                .OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            return null;
        }
    }

    public async Task WriteAsync(StoragePath path, Stream contentStream, CancellationToken cancellationToken = default)
    {
        await _client
            .GetBlobContainerClient(_containerName)
            .CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        await _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(path.WithoutLeadingSlash)
            .UploadAsync(contentStream, true, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        { 
            await _client
                .GetBlobContainerClient(_containerName)
                .DeleteBlobAsync(path.WithoutLeadingSlash, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            // Ignore?
        }
    }

    public async Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        var containerClient = _client.GetBlobContainerClient(_containerName);

        var sourceBlob = containerClient.GetBlobClient(sourcePath.WithoutLeadingSlash);
        var destinationBlob = containerClient.GetBlobClient(destinationPath.WithoutLeadingSlash);

        await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri, cancellationToken: cancellationToken);
        await sourceBlob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Uri> GetDownloadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default)
    {
        var blobClient = _client
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(path.WithoutLeadingSlash);

        if (!blobClient.CanGenerateSasUri)
            return await Task.FromResult(new Uri(_client.Uri, $"{_containerName}{path}"));

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

    private static Uri GetServiceUri(string accountName)
    {
        return new Uri($"https://{accountName}.blob.core.windows.net/");
    }
}
