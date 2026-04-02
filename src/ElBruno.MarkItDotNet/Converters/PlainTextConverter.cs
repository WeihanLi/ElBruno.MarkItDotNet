namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts plain text (.txt) files to Markdown.
/// Plain text is already valid Markdown, so the content is returned as-is.
/// </summary>
public class PlainTextConverter : IMarkdownConverter
{
    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<string> ConvertAsync(Stream fileStream, string fileExtension)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        using var reader = new StreamReader(fileStream, leaveOpen: true);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }
}
