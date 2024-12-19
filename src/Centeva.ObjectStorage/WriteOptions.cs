namespace Centeva.ObjectStorage;

/// <summary>
/// Options for writing storage entries
/// </summary>
/// <param name="ContentType">The content type of the entry</param>
/// <param name="Metadata">The metadata of the entry</param>
public record struct WriteOptions(string? ContentType, IReadOnlyDictionary<string, string>? Metadata);