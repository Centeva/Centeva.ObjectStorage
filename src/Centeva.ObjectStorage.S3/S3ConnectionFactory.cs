﻿using Centeva.ObjectStorage.Connections;

namespace Centeva.ObjectStorage.S3;

public class S3ConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "s3";
    private const string Endpoint = "endpoint";
    private const string Bucket = "bucket";
    private const string AccessKey = "accessKey";
    private const string SecretKey = "secretKey";
    private const string Region = "region";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName)
            return null;

        var bucket = connectionString.GetRequired(Bucket);
        var region = connectionString.Get(Region);
        var endpoint = connectionString.Get(Endpoint);
        var accessKey = connectionString.GetRequired(AccessKey);
        var secretKey = connectionString.GetRequired(SecretKey);

        return new S3ObjectStorage(bucket, region, endpoint, accessKey, secretKey);
    }
}
