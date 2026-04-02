namespace ElBruno.MarkItDotNet;

/// <summary>
/// Contract for converting a specific file format to Markdown.
/// </summary>
public interface IMarkdownConverter
{
    /// <summary>
    /// Determines whether this converter can handle the given file extension.
    /// </summary>
    /// <param name="fileExtension">File extension including the leading dot (e.g., ".txt").</param>
    bool CanHandle(string fileExtension);

    /// <summary>
    /// Converts the content of a stream to Markdown.
    /// </summary>
    /// <param name="fileStream">The input stream containing file content.</param>
    /// <param name="fileExtension">File extension including the leading dot (e.g., ".txt").</param>
    /// <returns>Markdown string.</returns>
    Task<string> ConvertAsync(Stream fileStream, string fileExtension);
}

/// <summary>
/// Convenience extension methods for <see cref="IMarkdownConverter"/>.
/// </summary>
public static class MarkdownConverterExtensions
{
    /// <summary>
    /// Converts a file at the given path to Markdown.
    /// </summary>
    public static async Task<string> ConvertAsync(this IMarkdownConverter converter, string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        using var stream = File.OpenRead(filePath);
        return await converter.ConvertAsync(stream, extension).ConfigureAwait(false);
    }
}
