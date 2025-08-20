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
}
