using System.Net;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

using static Google.Apis.Requests.BatchRequest;

namespace Centeva.ObjectStorage.GCP;

public class GoogleObjectStorage : ISignedUrlObjectStorage
{
    private readonly string _bucketName;
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;

    public GoogleObjectStorage(string bucketName, GoogleCredential credential)
    {
        _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));

        _storageClient = StorageClient.Create(credential);
        _urlSigner = UrlSigner.FromCredential(credential.UnderlyingCredential as ServiceAccountCredential);
    }

    private GoogleObjectStorage(string bucketName, string credentialsJsonString)
    {
        _bucketName = bucketName;

        GoogleCredential nativeCredential = GoogleCredential.FromJson(credentialsJsonString);
        _storageClient = StorageClient.Create(nativeCredential);
        _urlSigner = UrlSigner.FromCredential(nativeCredential.UnderlyingCredential as ServiceAccountCredential);
    }

    /// <summary>
    /// Create a new GoogleObjectStorage using credentials in JSON format
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="credentialsJsonString"></param>
    /// <returns></returns>
    public static GoogleObjectStorage CreateFromCredentialsJson(string bucket, string credentialsJsonString)
    {
        _ = bucket ?? throw new ArgumentNullException(nameof(bucket));
        _ = credentialsJsonString ?? throw new ArgumentNullException(nameof(credentialsJsonString));

        return new GoogleObjectStorage(bucket, credentialsJsonString);
    }

    /// <summary>
    /// Create a new GoogleObjectStorage using credentials from a file
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="credentialsFilePath"></param>
    /// <returns></returns>
    public static GoogleObjectStorage CreateFromCredentialsFile(string bucketName, string credentialsFilePath)
    {
        _ = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        _ = credentialsFilePath ?? throw new ArgumentNullException(nameof(credentialsFilePath));

        return new GoogleObjectStorage(bucketName, File.ReadAllText(credentialsFilePath));
    }

    //public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    //{
    //    var list = new List<string>();
    //    var request = _storageClient.Service.Objects.List(_bucketName);

    //    do
    //    {
    //        var page = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);

    //        if (page.Items != null)
    //        {
    //            list.AddRange(page.Items.Select(x => StoragePath.Normalize(x.Name)));
    //        }

    //        request.PageToken = page.NextPageToken;
    //    }
    //    while (request.PageToken != null && !cancellationToken.IsCancellationRequested);

    //    return list;
    

    public async Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, CancellationToken cancellationToken = default)
    {
        if (path is { IsFolder: false })
        {
            throw new ArgumentException("Path needs to be a folder", nameof(path));
        }

        var prefix = StoragePath.IsRootPath(path) ? null : path!.WithoutLeadingSlash;

        var blobs = _storageClient.ListObjectsAsync(_bucketName, prefix);

        var entries = new List<StorageEntry>();
        await foreach (var blob in blobs)
        {
            entries.Add(ToStorageEntry(blob));
        }

        return entries;
    }

    public async Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient
                .GetObjectAsync(_bucketName, path.WithoutLeadingSlash, null, cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _storageClient
                .GetObjectAsync(_bucketName, path.WithoutLeadingSlash, null, cancellationToken)
                .ConfigureAwait(false);

            return ToStorageEntry(response);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var ms = new MemoryStream();

        try
        {
            await _storageClient.DownloadObjectAsync(_bucketName, path.WithoutLeadingSlash, ms, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public async Task WriteAsync(StoragePath path, Stream contentStream, CancellationToken cancellationToken = default)
    {
        await _storageClient
            .UploadObjectAsync(_bucketName, path.WithoutLeadingSlash, null, contentStream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        try
        {
            await _storageClient
                .DeleteObjectAsync(_bucketName, path.WithoutLeadingSlash, null, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
        }
    }

    public async Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        await _storageClient.CopyObjectAsync(_bucketName, sourcePath.WithoutLeadingSlash, _bucketName, destinationPath.WithoutLeadingSlash, null, cancellationToken)
            .ConfigureAwait(false);

        await _storageClient.DeleteObjectAsync(_bucketName, sourcePath.WithoutLeadingSlash, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Uri> GetDownloadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        return new Uri(await _urlSigner.SignAsync(_bucketName, path.WithoutLeadingSlash, TimeSpan.FromSeconds(lifetimeInSeconds), HttpMethod.Get, cancellationToken: cancellationToken));
    }

    private StorageEntry ToStorageEntry(Google.Apis.Storage.v1.Data.Object blob) =>
        new(blob.Name)
        {
            CreationTime = blob.TimeCreatedDateTimeOffset,
            LastModificationTime = blob.UpdatedDateTimeOffset,
            SizeInBytes = (long?)blob.Size
        };
}
