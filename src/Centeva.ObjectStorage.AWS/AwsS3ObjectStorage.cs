using System.Collections.ObjectModel;
using System.Net;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;

namespace Centeva.ObjectStorage.AWS;

public class AwsS3ObjectStorage : IObjectStorage, ISupportsSignedUrls, ISupportsMetadata
{
    private readonly IAmazonS3 _client;
    private readonly ITransferUtility _fileFileTransferUtility;
    private bool _clientInitialized = false;

    private readonly string _bucketName;

    private const string MetadataKeyPrefix = "x-amz-meta-";

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

    public async Task<IReadOnlyCollection<StorageEntry>> ListAsync(StoragePath? path = null, ListOptions options = default, CancellationToken cancellationToken = default)
    {
        if (path is { IsFolder: false })
        {
            throw new ArgumentException("Path needs to be a folder", nameof(path));
        }

        var client = await GetClientAsync().ConfigureAwait(false);

        var request = new ListObjectsV2Request()
        {
            BucketName = _bucketName,
            Prefix = path?.WithoutLeadingSlash,
            Delimiter = options.Recurse ? null : "/"
        };

        var entries = new List<StorageEntry>();
        ListObjectsV2Response response;

        do
        {
            response = await client.ListObjectsV2Async(request, cancellationToken).ConfigureAwait(false);

            foreach (var s3object in response.S3Objects)
            {
                var entry = ToStorageEntry(s3object);
                if (options.IncludeMetadata)
                {
                    var metadataRes = await client.GetObjectMetadataAsync(_bucketName, s3object.Key, cancellationToken).ConfigureAwait(false);

                    entry.Metadata = ConvertMetadata(metadataRes);
                    entry.ContentType = metadataRes.Headers.ContentType;
                }

                entries.Add(entry);
            }

            entries.AddRange(response.CommonPrefixes.Select(x => new StorageEntry(x)));
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated);

        if (options.Recurse)
        {
            entries.InsertRange(0, FolderHelper.GetImpliedFolders(entries, path));
        }

        return entries.AsReadOnly();
    }

    public async Task<bool> ExistsAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            await client.GetObjectMetadataAsync(_bucketName, path.WithoutLeadingSlash, cancellationToken).ConfigureAwait(false);

            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
        }

        return false;
    }

    public async Task<StorageEntry?> GetAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        try
        {
            var response = await client.GetObjectMetadataAsync(_bucketName, path.WithoutLeadingSlash, cancellationToken);

            return new StorageEntry(path)
            {
                CreationTime = response.LastModified,
                LastModificationTime = response.LastModified,
                SizeInBytes = response.ContentLength,
                ContentType = response.Headers.ContentType,
                Metadata = ConvertMetadata(response)
            };
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Stream?> OpenReadAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var response = await GetObjectAsync(path.WithoutLeadingSlash).ConfigureAwait(false);

        return response?.ResponseStream;
    }
    
    public async Task WriteAsync(StoragePath path, Stream contentStream, WriteOptions? writeOptions = null, CancellationToken cancellationToken = default)
    {
        var request = new TransferUtilityUploadRequest
        {
            InputStream = contentStream,
            ContentType = writeOptions?.ContentType,
            Key = path.WithoutLeadingSlash,
            BucketName = _bucketName
        };

        if (writeOptions?.ContentDisposition is not null)
        {
            request.Headers.ContentDisposition = writeOptions.Value.ContentDisposition.ToString();
        }

        if (writeOptions?.Metadata != null)
        {
            foreach (var key in writeOptions.Value.Metadata.Keys)
            {
                request.Metadata[key] = writeOptions.Value.Metadata[key];
            }
        }

        await _fileFileTransferUtility.UploadAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(StoragePath path, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        await client.DeleteObjectAsync(_bucketName, path.WithoutLeadingSlash, cancellationToken).ConfigureAwait(false);
    }

    public async Task RenameAsync(StoragePath sourcePath, StoragePath destinationPath, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        await client.CopyObjectAsync(_bucketName, sourcePath.WithoutLeadingSlash, _bucketName, destinationPath.WithoutLeadingSlash, cancellationToken)
            .ConfigureAwait(false);

        await client.DeleteObjectAsync(_bucketName, sourcePath.WithoutLeadingSlash, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Uri> GetDownloadUrlAsync(StoragePath path, SignedUrlOptions? options = null, CancellationToken cancellationToken = default)
    {
        var urlOptions = options ?? new SignedUrlOptions();
        var client = await GetClientAsync().ConfigureAwait(false);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = path.WithoutLeadingSlash,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(urlOptions.Duration)
        };

        var result = new Uri(await client.GetPreSignedURLAsync(request));

        if (client.Config.UseHttp)
        {
            var builder = new UriBuilder(result) { Scheme = Uri.UriSchemeHttp };
            result = builder.Uri;
        }

        return result;
    }

    public Task<Uri> GetDownloadUrlAsync(StoragePath path, int lifetimeInSeconds = 86400, CancellationToken cancellationToken = default)
        => GetDownloadUrlAsync(path, new SignedUrlOptions { Duration = TimeSpan.FromSeconds(lifetimeInSeconds) }, cancellationToken);

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

    private static StorageEntry ToStorageEntry(S3Object blob)
    {
        return new StorageEntry(blob.Key)
        {
            CreationTime = blob.LastModified,
            LastModificationTime = blob.LastModified,
            SizeInBytes = blob.Size,
            Metadata = null
        };
    }

    private static Dictionary<string, string> ConvertMetadata(GetObjectMetadataResponse response)
    {
        var metadata = new Dictionary<string, string>();
        foreach (var key in response.Metadata.Keys)
        {
            string value = response.Metadata[key];
            if (key.StartsWith(MetadataKeyPrefix))
            {
                var ourKey = key.Substring(MetadataKeyPrefix.Length);
                metadata[ourKey] = value;
            }
        }

        return metadata;
    }

    public async Task UpdateMetadataAsync(StoragePath path, UpdateStorageEntryRequest request, CancellationToken cancellationToken = default)
    {
        var client = await GetClientAsync().ConfigureAwait(false);

        var copyRequest = new CopyObjectRequest
        {
            SourceBucket = _bucketName,
            SourceKey = path.WithoutLeadingSlash,
            DestinationBucket = _bucketName,
            DestinationKey = path.WithoutLeadingSlash,
            MetadataDirective = S3MetadataDirective.REPLACE,
        };

        foreach (var key in request.Metadata.Keys)
        {
            copyRequest.Metadata.Add(key, request.Metadata[key]);
        }

        await client.CopyObjectAsync(copyRequest, cancellationToken).ConfigureAwait(false);
    }
}
