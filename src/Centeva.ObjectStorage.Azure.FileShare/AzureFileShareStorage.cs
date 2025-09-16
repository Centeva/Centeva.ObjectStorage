
using System.Collections.ObjectModel;

using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Sas;

namespace Centeva.ObjectStorage.Azure.FileShare;

public class AzureFileShareStorage : IObjectStorage, ISupportsSignedUrls, ISupportsMetadata
{
    private readonly ShareClient _client;
    private readonly string? _shareName = null;

    public AzureFileShareStorage(string accountName, string accountKey, string container, Uri? serviceUri = null)
    {
        _shareName = container;
        StorageSharedKeyCredential credentials = new(accountName, accountKey);
        _client = new ShareClient(serviceUri ?? GetServiceUri(accountName), credentials);
    }

    public AzureFileShareStorage(string accountName, string container, TokenCredential identity, Uri? serviceUri)
    {
        _shareName = container;
        _client = new ShareClient(serviceUri ?? GetServiceUri(accountName), identity);
    }

    public async Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, ListOptions options = default, CancellationToken cancellationToken = default)
    {
        if (path is { IsFolder: false })
            throw new ArgumentException("Path needs to be a folder", nameof(path));

        var directoryClient = _client.GetDirectoryClient(path?.WithoutLeadingSlash ?? string.Empty);
        var entries = new List<StorageEntry>();

        await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(cancellationToken: cancellationToken))
        {
            if (item.IsDirectory)
                entries.Add(new StorageEntry(item.Name + "/"));
            else
            {
                var fileClient = directoryClient.GetFileClient(item.Name);
                var properties = await fileClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                entries.Add(ToStorageEntry(item.Name, properties.Value));
            }
        }

        if (options.Recurse)
            entries.InsertRange(0, FolderHelper.GetImpliedFolders(entries, path));

        return entries.AsReadOnly();
    }

    public async Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var fileClient = _client.GetDirectoryClient(path.Folder).GetFileClient(path.Name);

        try
        {
            await fileClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == ShareErrorCode.ResourceNotFound)
        {
            return false;
        }
    }

    public async Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var fileClient = _client.GetDirectoryClient(path.Folder).GetFileClient(path.Name);

        try
        {
            var properties = await fileClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return ToStorageEntry(path, properties.Value);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == ShareErrorCode.ResourceNotFound)
        {
            return null;
        }
    }

    public async Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileClient = _client.GetDirectoryClient(path.Folder).GetFileClient(path.Name);
            return await fileClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == ShareErrorCode.ResourceNotFound)
        {
            return null;
        }
    }

    public async Task WriteAsync(StoragePath path, Stream contentStream, WriteOptions? writeOptions = null, CancellationToken cancellationToken = default)
    {
        var directoryClient = _client.GetDirectoryClient(path.Folder);
        await directoryClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var fileClient = directoryClient.GetFileClient(path.Name);
        await fileClient.CreateAsync(contentStream.Length, cancellationToken: cancellationToken);

        int blockSize = 4 * 1024 * 1024;    // 4MB
        long offset = 0;                    // HttpRange offset
        using BinaryReader reader = new(contentStream);
        while (true)
        {
            byte[] buffer = reader.ReadBytes(blockSize);
            if (buffer.Length == 0)
                break;

            MemoryStream uploadChunk = new();
            uploadChunk.Write(buffer, 0, buffer.Length);
            uploadChunk.Position = 0;

            HttpRange httpRange = new(offset, buffer.Length);
            await fileClient.UploadRangeAsync(httpRange, uploadChunk, cancellationToken: cancellationToken);
            offset += buffer.Length;
        }

        if (writeOptions?.Metadata != null)
        {
            var metadata = writeOptions?.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            await fileClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
        }
    }

    public async Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileClient = _client.GetDirectoryClient(path.Folder).GetFileClient(path.Name);
            await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == ShareErrorCode.ResourceNotFound)
        {
            // Ignore?
        }
    }

    public async Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        var sourceFileClient = _client.GetDirectoryClient(sourcePath.Folder).GetFileClient(sourcePath.Name);
        var destinationFileClient = _client.GetDirectoryClient(destinationPath.Folder).GetFileClient(destinationPath.Name);

        await destinationFileClient.StartCopyAsync(sourceFileClient.Uri, cancellationToken: cancellationToken);
        await sourceFileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<Uri> GetDownloadUrlAsync(StoragePath path, SignedUrlOptions? options = null, CancellationToken cancellationToken = default)
    {
        var urlOptions = options ?? new SignedUrlOptions();
        var fileClient = _client.GetDirectoryClient(path.Folder).GetFileClient(path.Name);

        if (!fileClient.CanGenerateSasUri)
            return await Task.FromResult(new Uri(_client.Uri, $"{_shareName}/{path}"));

        ShareSasBuilder sasBuilder = new()
        {
            ShareName = _shareName,
            FilePath = path.WithoutLeadingSlash,
            Resource = "f",
            ExpiresOn = DateTimeOffset.UtcNow.Add(urlOptions.Duration),
            ContentDisposition = urlOptions.ContentDisposition?.ToString()
        };
        sasBuilder.SetPermissions(ShareFileSasPermissions.Read);

        Uri sasUri = fileClient.GenerateSasUri(sasBuilder);
        return sasUri;
    }

    public async Task UpdateMetadataAsync(StoragePath path, UpdateStorageEntryRequest request, CancellationToken cancellationToken = default)
    {
        ShareDirectoryClient rootDirectory = _client.GetRootDirectoryClient();
        ShareFileClient fileClient = rootDirectory.GetFileClient(path.WithoutLeadingSlash);
        await fileClient.SetMetadataAsync(request.Metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static Uri GetServiceUri(string accountName)
    {
        return new Uri($"https://{accountName}.file.core.windows.net/");
    }

    private static StorageEntry ToStorageEntry(string path, ShareFileProperties properties)
    {
        return new StorageEntry(path)
        {
            LastModificationTime = properties.LastModified,
            SizeInBytes = properties.ContentLength,
            ContentType = properties.ContentType,
            Metadata = new ReadOnlyDictionary<string, string>(properties.Metadata)
        };
    }

    private static StorageEntry ToStorageEntry(StoragePath path, ShareFileProperties properties)
    {
        return new StorageEntry(path.Full)
        {
            LastModificationTime = properties.LastModified,
            SizeInBytes = properties.ContentLength,
            ContentType = properties.ContentType,
            Metadata = new ReadOnlyDictionary<string, string>(properties.Metadata)
        };
    }
}
