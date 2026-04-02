namespace ElBruno.MarkItDotNet;

/// <summary>
/// Registry of <see cref="IMarkdownConverter"/> implementations.
/// Resolves the appropriate converter for a given file extension.
/// </summary>
public class ConverterRegistry
{
    private readonly List<IMarkdownConverter> _converters = [];

    /// <summary>
    /// Registers a converter in the registry.
    /// </summary>
    public void Register(IMarkdownConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        _converters.Add(converter);
    }

    /// <summary>
    /// Resolves the first converter that can handle the given file extension.
    /// </summary>
    /// <param name="extension">File extension including the leading dot (e.g., ".txt").</param>
    /// <returns>A matching converter, or null if none can handle the extension.</returns>
    public IMarkdownConverter? Resolve(string extension)
    {
        var normalized = extension.ToLowerInvariant();
        return _converters.FirstOrDefault(c => c.CanHandle(normalized));
    }

    /// <summary>
    /// Returns all registered converters.
    /// </summary>
    public IReadOnlyList<IMarkdownConverter> GetAll() => _converters.AsReadOnly();
}
