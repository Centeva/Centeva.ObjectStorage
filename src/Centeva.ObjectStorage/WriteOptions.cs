using System.Net.Mime;

namespace Centeva.ObjectStorage;

/// <summary>
/// Options for writing storage entries
/// </summary>
public record struct WriteOptions
{
    /// <summary>
    /// (deprecated) Initializes a new instance of the <see cref="WriteOptions"/> class with the specified content type and metadata.
    /// </summary>
    /// <param name="contentType">The MIME type of the content to be written. Can be <see langword="null"/> if no specific content type is
    /// required.</param>
    /// <param name="metadata">A read-only dictionary containing metadata key-value pairs associated with the content. Can be <see
    /// langword="null"/> if no metadata is provided.</param>
    public WriteOptions(string? contentType, IReadOnlyDictionary<string, string>? metadata)
    {
        ContentType = contentType;
        Metadata = metadata;
    }

    /// <summary>
    /// The MIME content type of the entry
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Value of a Content-Disposition header for the entry.
    /// </summary>
    public ContentDisposition? ContentDisposition { get; set; }

    /// <summary>
    /// Set of metadata key/value pairs to associate with the entry.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}