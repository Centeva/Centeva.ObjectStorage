using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Centeva.ObjectStorage.S3;

public class S3ObjectStorage : ISignedUrlObjectStorage
{
    private readonly IAmazonS3 _client;
    private readonly ITransferUtility _fileFileTransferUtility;
    private bool _clientInitialized = false;

    private readonly string _bucketName;

    public S3ObjectStorage(string bucketName, string? region, string? endpoint, string accessKey, string secretKey)
    {
        _bucketName = bucketName;

        var config = CreateConfig(region, endpoint);
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        _client = new AmazonS3Client(credentials, config);
        _fileFileTransferUtility = new TransferUtility(_client);
    }

    /// <summary>
    /// Constructor used for unit testing, since you can pass a mocked IAmazonS3
    /// </summary>
    internal S3ObjectStorage(IAmazonS3 client, ITransferUtility fileTransferUtility, string bucketName)
    {
        _client = client;
        _fileFileTransferUtility = fileTransferUtility;
        _bucketName = bucketName;
    }

    public async Task<Stream?> OpenReadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var response = await GetObjectAsync(objectName).ConfigureAwait(false);

        return response?.ResponseStream;
    }

    public async Task WriteAsync(string objectName, Stream dataStream, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var request = new TransferUtilityUploadRequest {
            InputStream = dataStream,
            Key = objectName,
            BucketName = _bucketName
        };

        await _fileFileTransferUtility.UploadAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var client = await GetClientAsync().ConfigureAwait(false);

        await client.DeleteObjectAsync(_bucketName, objectName, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string objectName, CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            await client.GetObjectMetadataAsync(_bucketName, objectName, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
        }

        return false;
    }

    public async Task<Uri> GetDownloadUrlAsync(string objectName, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        objectName = StoragePath.Normalize(objectName, true);

        var client = await GetClientAsync().ConfigureAwait(false);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectName,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddSeconds(lifetimeInSeconds)
        };

        var result = new Uri(client.GetPreSignedURL(request));

        if (client.Config.UseHttp)
        {
            var builder = new UriBuilder(result) { Scheme = Uri.UriSchemeHttp };
            result = builder.Uri;
        }

        return result;
    }

    private static AmazonS3Config CreateConfig(string? region, string? endpointUrl)
    {
        var config = new AmazonS3Config();
        if (region != null)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);
            config.RegionEndpoint = regionEndpoint;
        }

        if (endpointUrl != null)
        {
            config.ServiceURL = endpointUrl;
            // Use "endpoint/bucket/path" URLs instead of "bucket.endpoint/path" to avoid DNS issues and support things like MinIO
            config.ForcePathStyle = true;
            config.UseHttp = !endpointUrl.StartsWith("https");
        }

        return config;
    }

    private async Task<IAmazonS3> GetClientAsync()
    {
        if (!_clientInitialized)
        {
            try
            {
                var request = new PutBucketRequest { BucketName = _bucketName };
                await _client.PutBucketAsync(request);
                _clientInitialized = true;
            }
            catch (AmazonS3Exception e) when (e.ErrorCode == "BucketAlreadyOwnedByYou")
            {
                // Bucket already exists
                _clientInitialized = true;
            }
        }

        return _client;
    }

    private async Task<GetObjectResponse?> GetObjectAsync(string fullPath)
    {
        var request = new GetObjectRequest { BucketName = _bucketName, Key = fullPath };

        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            var response = await client.GetObjectAsync(request).ConfigureAwait(false);
            return response;
        }
        catch (AmazonS3Exception e)
        {
            if (FileNotFound(e))
                return null;

            throw;
        }
    }

    private static bool FileNotFound(AmazonS3Exception exception)
    {
        return exception.ErrorCode == "NoSuchKey";
    }

    public async Task<IEnumerable<string>> ListAsync(int pageSize, CancellationToken cancellationToken = default)
    {

        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            return await client.GetAllObjectKeysAsync(_bucketName, "", null).ConfigureAwait(false);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<string>();
        }
    }
}
