using System.Net;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

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

    public async Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var ms = new MemoryStream();

        try
        {
            await _storageClient.DownloadObjectAsync(_bucketName, objectName, ms, cancellationToken: cancellationToken);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public async Task WriteAsync(string objectName, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);
    
        await _storageClient
            .UploadObjectAsync(_bucketName, objectName, contentType, dataStream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        try
        {
            await _storageClient
                .DeleteObjectAsync(_bucketName, objectName, null, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
        }
    }

    public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();
        var request = _storageClient.Service.Objects.List(_bucketName);

        do
        {
            var page = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            if (page.Items != null)
            {
                list.AddRange(page.Items.Select(x => x.Name));
            }

            request.PageToken = page.NextPageToken;
        }
        while (request.PageToken != null && !cancellationToken.IsCancellationRequested);

        return list;
    }

    public async Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        try
        {
            await _storageClient
                .GetObjectAsync(_bucketName, objectName, null, cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
    
    public async Task<Uri> GetDownloadUrlAsync(string objectName, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        return new Uri(await _urlSigner.SignAsync(_bucketName, objectName, TimeSpan.FromSeconds(lifetimeInSeconds), HttpMethod.Get, cancellationToken: cancellationToken));
    }
}
