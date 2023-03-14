using System.Net;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;

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

    public Task<Stream?> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

        await _storageClient.DeleteObjectAsync(_bucketName, objectName, null, cancellationToken);
    }

    public async Task<IEnumerable<string>> ListAsync(int pageSize, CancellationToken cancellationToken = default)
    {
        var lookup = _storageClient.ListObjectsAsync(_bucketName);
        return (await lookup.ReadPageAsync(pageSize, cancellationToken)).Select(x => x.Name);
    }

    public async Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        try
        {
            await _storageClient.GetObjectAsync(_bucketName, objectName, null, cancellationToken).ConfigureAwait(false);

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
