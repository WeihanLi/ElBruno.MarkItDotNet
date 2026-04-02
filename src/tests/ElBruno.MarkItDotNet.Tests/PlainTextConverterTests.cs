using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class PlainTextConverterTests
{
    private readonly PlainTextConverter _converter = new();

    [Fact]
    public void CanHandle_Txt_ReturnsTrue()
    {
        _converter.CanHandle(".txt").Should().BeTrue();
    }

    [Fact]
    public void CanHandle_OtherExtension_ReturnsFalse()
    {
        _converter.CanHandle(".pdf").Should().BeFalse();
    }

    [Fact]
    public void CanHandle_IsCaseInsensitive()
    {
        _converter.CanHandle(".TXT").Should().BeTrue();
        _converter.CanHandle(".Txt").Should().BeTrue();
    }

    [Fact]
    public async Task ConvertAsync_ReturnsFileContent()
    {
        using var stream = new MemoryStream("Hello, Markdown!"u8.ToArray());

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().Be("Hello, Markdown!");
    }

    [Fact]
    public async Task ConvertAsync_EmptyStream_ReturnsEmptyString()
    {
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _converter.ConvertAsync(stream, ".txt");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => _converter.ConvertAsync(null!, ".txt");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
