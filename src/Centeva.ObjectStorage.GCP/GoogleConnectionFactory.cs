using Centeva.ObjectStorage.ConnectionString;

namespace Centeva.ObjectStorage.GCP;

public class GoogleConnectionFactory : IConnectionFactory
{
    private const string ProviderName = "google";
    private const string Bucket = "bucket";
    private const string Credentials = "credentials";
    private const string CredentialsFilePath = "credentialsFilePath";

    public IObjectStorage? CreateConnection(ObjectStorageConnectionString connectionString)
    {
        if (connectionString.ProviderName != ProviderName)
            return null;

        var bucketName = connectionString.GetRequired(Bucket);
        var credentialsFilePath = connectionString.Get(CredentialsFilePath);
        var credentials = connectionString.Get(Credentials);

        if (credentialsFilePath != null)
        {
            return GoogleObjectStorage.CreateFromCredentialsFile(bucketName, credentialsFilePath);
        }

        if (credentials != null)
        {
            return GoogleObjectStorage.CreateFromCredentialsJson(bucketName, credentials.Base64Decode());
        }

        return null;
    }
}
