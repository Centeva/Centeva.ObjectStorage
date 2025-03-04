using Microsoft.Extensions.Configuration;

namespace Centeva.ObjectStorage.IntegrationTests;

public class TestSettings
{
    public string? AwsTestBucketName { get; set; }
    public string? AwsTestBucketRegion { get; set; } = "us-west-1";
    public string? AwsAccessKey { get; set; }
    public string? AwsSecretKey { get; set; }

    public string? AzureStorageTestContainerName { get; set; }
    public string? AzureAccountName { get; set; }
    public string? AzureAccountKey { get; set; }

    public string? GoogleStorageTestBucketName { get; set; }
    public string? GoogleStorageCredentialsJson { get; set; }

    private static TestSettings? _instance;
    public static TestSettings Instance
    {
        get
        {
            if (_instance is null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("testsettings.json", optional: true)
                    .AddUserSecrets<TestSettings>(optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                _instance = config.Get<TestSettings>();
            }

            return _instance!;
        }
    }
}
