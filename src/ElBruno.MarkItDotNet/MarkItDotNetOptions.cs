namespace ElBruno.MarkItDotNet;

/// <summary>
/// Configuration options for the MarkItDotNet library.
/// </summary>
public class MarkItDotNetOptions
{
    /// <summary>
    /// Enables OCR support for image and scanned-PDF conversion. Default is false (v2 feature).
    /// </summary>
    public bool EnableOcr { get; set; }
}
