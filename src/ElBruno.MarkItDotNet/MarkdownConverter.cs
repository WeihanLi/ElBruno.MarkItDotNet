using ElBruno.MarkItDotNet.Converters;

namespace ElBruno.MarkItDotNet;

/// <summary>
/// Simple façade for converting files to Markdown.
/// For advanced usage and DI scenarios, prefer <see cref="MarkdownService"/>.
/// </summary>
public class MarkdownConverter
{
    private readonly MarkdownService _service;

    /// <summary>
    /// Creates a new converter with all built-in converters registered.
    /// </summary>
    public MarkdownConverter()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());
        _service = new MarkdownService(registry);
    }

    /// <summary>
    /// Converts the content of a file to Markdown.
    /// </summary>
    /// <param name="filePath">Path to the file to convert.</param>
    /// <returns>The Markdown representation of the file content.</returns>
    public string ConvertToMarkdown(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var result = _service.ConvertAsync(filePath).GetAwaiter().GetResult();
        if (!result.Success)
        {
            throw new NotSupportedException(result.ErrorMessage);
        }

        return result.Markdown;
    }
}
