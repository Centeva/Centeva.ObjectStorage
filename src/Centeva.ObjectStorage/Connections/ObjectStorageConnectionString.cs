namespace Centeva.ObjectStorage.Connections;

public class ObjectStorageConnectionString
{
    public string ConnectionString { get; }
    public string ProviderName { get; private set; } = String.Empty;

    private readonly Dictionary<string, string?> _parameters = new();

    private const string ProviderNameSeparator = "://";
    private static readonly char ParameterSeparator = ';';
    private static readonly char PairSeparator = '=';

    public ObjectStorageConnectionString(string connectionString, bool parseProviderName = true, bool urlDecode = true)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        Parse(connectionString, parseProviderName, urlDecode);
    }

    public string GetRequired(string parameterName)
    {
        return _parameters.TryGetValue(parameterName, out string? value)
            ? value ?? throw new ArgumentException($"Connection string requires non-empty '{parameterName}' parameter.")
            : throw new ArgumentException($"Connection string requires '{parameterName}' parameter.");
    }

    public string? Get(string parameterName)
    {
        return _parameters.TryGetValue(parameterName, out string? value) ? value : null;
    }

    private void Parse(string connectionString, bool parseProviderName, bool urlDecodeParameter)
    {
        int indexOfProviderNameSeparator = -1;
        if (parseProviderName)
        {
            indexOfProviderNameSeparator = connectionString.IndexOf(ProviderNameSeparator, StringComparison.Ordinal);
            if (indexOfProviderNameSeparator == -1)
            {
                throw new ArgumentException("Invalid connection string format", nameof(connectionString));
            }

            ProviderName = connectionString[..indexOfProviderNameSeparator] ?? string.Empty;
        }

        string parameterString = parseProviderName
            ? connectionString[(indexOfProviderNameSeparator + ProviderNameSeparator.Length)..]
            : connectionString;

        string[] parameterPairs = parameterString.Split(ParameterSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in parameterPairs)
        {
            string[] splitPair = pair.Split(PairSeparator, 2);
            string key = splitPair[0];
            var value = urlDecodeParameter ? splitPair[1].UrlDecode() : splitPair[1];

            _parameters[key] = value;
        }
    }
}
