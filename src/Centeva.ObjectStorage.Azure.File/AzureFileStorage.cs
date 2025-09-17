
using System.Collections.ObjectModel;

using Azure;
using Azure.Core;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Sas;

namespace Centeva.ObjectStorage.Azure.File;

public class AzureFileStorage : IObjectStorage, ISupportsSignedUrls, ISupportsMetadata
{
    private readonly ShareClient _client;
    private readonly string? _shareName = null;

    public AzureFileStorage(string accountName, string accountKey, string shareName, Uri? serviceUri = null)
    {
        _shareName = shareName;
        StorageSharedKeyCredential credentials = new(accountName, accountKey);
        _client = new ShareClient(new Uri(serviceUri ?? GetServiceUri(accountName), shareName), credentials);
    }

    public AzureFileStorage(string accountName, string shareName, TokenCredential identity, Uri? serviceUri)
    {
        _shareName = shareName;
        _client = new ShareClient(new Uri(serviceUri ?? GetServiceUri(accountName), shareName), identity);
    }

    public async Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, ListOptions options = default, CancellationToken cancellationToken = default)
    {
        if (path is { IsFolder: false })
            throw new ArgumentException("Path needs to be a folder", nameof(path));

        var directoryClient = _client.GetDirectoryClient(path?.WithoutLeadingSlash ?? string.Empty);

        var getFilesOptions = new ShareDirectoryGetFilesAndDirectoriesOptions()
        {
            IncludeExtendedInfo = true,
            Traits = ShareFileTraits.Timestamps
        };

        return await ListInternalAsync(directoryClient, getFilesOptions, options.Recurse, cancellationToken);
    }

    private async Task<IReadOnlyCollection<StorageEntry>> ListInternalAsync(ShareDirectoryClient directoryClient,
        ShareDirectoryGetFilesAndDirectoriesOptions options, bool recurse, CancellationToken cancellationToken = default)
    {
        var entries = new List<StorageEntry>();

        try
        {
            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync(options, cancellationToken: cancellationToken))
            {
                if (item.IsDirectory)
                {
                    entries.Add(new StorageEntry(StoragePath.Combine(directoryClient.Path, item.Name) + "/"));
                    if (recurse)
                    {
                        var subDirClient = directoryClient.GetSubdirectoryClient(item.Name);
                        entries.AddRange(await ListInternalAsync(subDirClient, options, recurse, cancellationToken));
                    }
                }
                else
                {
                    var fileClient = directoryClient.GetFileClient(item.Name);
                    var properties = await fileClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                    entries.Add(ToStorageEntry(StoragePath.Combine(directoryClient.Path, item.Name), properties.Value));
                }
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == ShareErrorCode.ResourceNotFound)
        {
            // Entry not found
        }

        return entries.AsReadOnly();
    }

    public async Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var fileClient = await GetFileClientAsync(path, false, cancellationToken);

        if (fileClient is null)
            return false;

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
        var fileClient = await GetFileClientAsync(path, false, cancellationToken);

        if (fileClient is null)
            return null;

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
            var fileClient = await GetFileClientAsync(path, false, cancellationToken);
            return fileClient is null ? null : await fileClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == ShareErrorCode.ResourceNotFound)
        {
            return null;
        }
    }

    public async Task WriteAsync(StoragePath path, Stream contentStream, WriteOptions? writeOptions = null, CancellationToken cancellationToken = default)
    {
        var fileClient = await GetFileClientAsync(path, true, cancellationToken);

        var createOptions = new ShareFileCreateOptions()
        {
            HttpHeaders = new ShareFileHttpHeaders()
            {
                ContentType = writeOptions?.ContentType,
                ContentDisposition = writeOptions?.ContentDisposition?.ToString(),
            }
        };

        await fileClient!.CreateAsync(contentStream.Length, createOptions, cancellationToken: cancellationToken);

        const int blockSize = 4 * 1024 * 1024;    // 4MB
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

    private async Task<ShareFileClient?> GetFileClientAsync(StoragePath path, bool createParents, CancellationToken cancellationToken) {
        string[] parts = StoragePath.Split(path);
        if (parts.Length == 0)
            return null;

        string rootFolderName = parts[0];

        ShareDirectoryClient directory = _client.GetDirectoryClient(rootFolderName);
        if (createParents)
            await directory.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        for (int i = 1; i < parts.Length - 1; i++) {
            string sub = parts[i];
            directory = directory.GetSubdirectoryClient(sub);

            if (createParents)
                await directory.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return directory.GetFileClient(parts[parts.Length - 1]);
    }

    private static StorageEntry ToStorageEntry(string path, ShareFileProperties properties)
    {
        return new StorageEntry(path)
        {
            CreationTime = properties.SmbProperties.FileCreatedOn,
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
            CreationTime = properties.SmbProperties.FileCreatedOn,
            LastModificationTime = properties.LastModified,
            SizeInBytes = properties.ContentLength,
            ContentType = properties.ContentType,
            Metadata = new ReadOnlyDictionary<string, string>(properties.Metadata)
        };
    }
}
