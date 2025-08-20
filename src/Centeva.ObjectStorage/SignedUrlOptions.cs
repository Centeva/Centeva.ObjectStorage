using System.Net.Mime;

namespace Centeva.ObjectStorage;

/// <summary>
/// Options for producing signed URLs
/// </summary>
public record struct SignedUrlOptions()
{
    /// <summary>
    /// How long the signed URL should be active
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Value of the Content-Disposition header sent when the signed URL is used to download the content.
    /// </summary>
    public ContentDisposition? ContentDisposition { get; set; }
}
