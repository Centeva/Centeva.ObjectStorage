using System.Net;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;

namespace Centeva.ObjectStorage.AWS;

public class AwsS3ObjectStorage : ISignedUrlObjectStorage
{
    private readonly IAmazonS3 _client;
    private readonly ITransferUtility _fileFileTransferUtility;
    private bool _clientInitialized = false;

    private readonly string _bucketName;

    public AwsS3ObjectStorage(string bucketName, string? region, string? endpoint, string accessKey, string secretKey)
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
    internal AwsS3ObjectStorage(IAmazonS3 client, ITransferUtility fileTransferUtility, string bucketName)
    {
        _client = client;
        _fileFileTransferUtility = fileTransferUtility;
        _bucketName = bucketName;
    }

    public async Task<Stream?> OpenReadAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        var response = await GetObjectAsync(storagePath.WithoutLeadingSlash).ConfigureAwait(false);

        return response?.ResponseStream;
    }

    public async Task WriteAsync(StoragePath storagePath, Stream dataStream, string? contentType = default, CancellationToken cancellationToken = default)
    {
        var request = new TransferUtilityUploadRequest
        {
            InputStream = dataStream,
            Key = storagePath.WithoutLeadingSlash,
            BucketName = _bucketName,
            ContentType = contentType
        };

        await _fileFileTransferUtility.UploadAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        await client.DeleteObjectAsync(_bucketName, storagePath.WithoutLeadingSlash, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(StoragePath storagePath, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            await client.GetObjectMetadataAsync(_bucketName, storagePath.WithoutLeadingSlash, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
        }

        return false;
    }

    public async Task RenameAsync(StoragePath storagePath, StoragePath newStoragePath, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        await client.CopyObjectAsync(_bucketName, storagePath.WithoutLeadingSlash, _bucketName, newStoragePath.WithoutLeadingSlash, cancellationToken)
            .ConfigureAwait(false);

        await client.DeleteObjectAsync(_bucketName, storagePath.WithoutLeadingSlash, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Uri> GetDownloadUrlAsync(StoragePath storagePath, int lifetimeInSeconds = 86400,
        CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = storagePath.WithoutLeadingSlash,
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
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, _bucketName);
            if (!bucketExists)
            {
                var request = new PutBucketRequest { BucketName = _bucketName };
                await _client.PutBucketAsync(request);
            }

            _clientInitialized = true;
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

    public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            var rawFiles = await client.GetAllObjectKeysAsync(_bucketName, "", null).ConfigureAwait(false);

            var files = rawFiles.Select(x => StoragePath.Normalize(x)).ToList();

            return files;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<string>();
        }
    }
}
