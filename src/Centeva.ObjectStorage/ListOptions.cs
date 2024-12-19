namespace Centeva.ObjectStorage;

/// <summary>
/// Options for listing storage entries
/// </summary>
/// <param name="Recurse">Set to true to recurse inside "folders"</param>
/// <param name="IncludeMetadata">Set to true to include entry metadata</param>
public record struct ListOptions(bool Recurse, bool IncludeMetadata);
