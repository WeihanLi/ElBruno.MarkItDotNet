using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

/// <summary>
/// Backward-compatibility tests for the <see cref="MarkdownConverter"/> façade.
/// </summary>
public class MarkdownConverterTests
{
    private readonly MarkdownConverter _converter = new();

    [Fact]
    public void ConvertToMarkdown_WithNullPath_ThrowsArgumentException()
    {
        var act = () => _converter.ConvertToMarkdown(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertToMarkdown_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => _converter.ConvertToMarkdown(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertToMarkdown_WithUnsupportedFormat_ThrowsNotSupportedException()
    {
        var act = () => _converter.ConvertToMarkdown("test.xyz");
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*not supported*");
    }
}
