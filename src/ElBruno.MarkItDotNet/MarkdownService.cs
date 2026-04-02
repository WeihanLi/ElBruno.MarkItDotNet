namespace ElBruno.MarkItDotNet;

/// <summary>
/// Main entry point for converting files to Markdown.
/// Delegates to registered <see cref="IMarkdownConverter"/> implementations via the <see cref="ConverterRegistry"/>.
/// </summary>
public class MarkdownService
{
    private readonly ConverterRegistry _registry;

    /// <summary>
    /// Creates a new <see cref="MarkdownService"/> with the given converter registry.
    /// </summary>
    public MarkdownService(ConverterRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>
    /// Converts a file at the given path to Markdown.
    /// </summary>
    /// <param name="filePath">Path to the file to convert.</param>
    /// <returns>A <see cref="ConversionResult"/> with the outcome.</returns>
    public async Task<ConversionResult> ConvertAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        var converter = _registry.Resolve(extension);
        if (converter is null)
        {
            return ConversionResult.Failure(
                $"File format '{extension}' is not supported.", extension);
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            var markdown = await converter.ConvertAsync(stream, extension).ConfigureAwait(false);
            return ConversionResult.Succeeded(markdown, extension);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ConversionResult.Failure(ex.Message, extension);
        }
    }

    /// <summary>
    /// Converts a stream to Markdown using the converter for the given file extension.
    /// </summary>
    /// <param name="stream">The input stream containing file content.</param>
    /// <param name="fileExtension">File extension including the leading dot (e.g., ".txt").</param>
    /// <returns>A <see cref="ConversionResult"/> with the outcome.</returns>
    public async Task<ConversionResult> ConvertAsync(Stream stream, string fileExtension)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        var extension = fileExtension.ToLowerInvariant();

        var converter = _registry.Resolve(extension);
        if (converter is null)
        {
            return ConversionResult.Failure(
                $"File format '{extension}' is not supported.", extension);
        }

        try
        {
            var markdown = await converter.ConvertAsync(stream, extension).ConfigureAwait(false);
            return ConversionResult.Succeeded(markdown, extension);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ConversionResult.Failure(ex.Message, extension);
        }
    }
}
