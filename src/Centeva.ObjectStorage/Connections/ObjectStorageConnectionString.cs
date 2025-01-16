namespace Centeva.ObjectStorage.Connections;

public class ObjectStorageConnectionString
{
    public string ConnectionString { get; }
    public string ProviderName { get; set; } = string.Empty;
    private readonly Dictionary<string, string?> _parameters = new(StringComparer.OrdinalIgnoreCase);

    private const string ProviderNameSeparator = "://";
    private static readonly char ParameterSeparator = ';';
    private static readonly char PairSeparator = '=';

    public ObjectStorageConnectionString(string connectionString)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        Parse(connectionString);
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

    protected virtual void Parse(string connectionString)
    {
        var indexOfProviderNameSeparator = connectionString.IndexOf(ProviderNameSeparator, StringComparison.Ordinal);
        if (indexOfProviderNameSeparator == -1)
        {
            throw new ArgumentException("Invalid connection string format", nameof(connectionString));
        }

        ProviderName = connectionString.Substring(0, indexOfProviderNameSeparator) ?? string.Empty;

        var parameterString = connectionString.Substring(indexOfProviderNameSeparator + ProviderNameSeparator.Length);
        ParseParameters(parameterString);
    }

    protected void ParseParameters(string parameterString, bool urlDecodeParameter = true)
    {
        string[] parameterPairs = parameterString.Split(new char[] { ParameterSeparator }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in parameterPairs)
        {
            string[] splitPair = pair.Split([PairSeparator], 2);
            string key = splitPair[0];
            var value = urlDecodeParameter ? splitPair[1].UrlDecode() : splitPair[1];

            _parameters[key] = value;
        }
    }
}
