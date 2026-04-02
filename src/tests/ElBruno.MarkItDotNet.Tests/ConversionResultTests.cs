using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class ConversionResultTests
{
    [Fact]
    public void Succeeded_ReturnsSuccessResult()
    {
        var result = ConversionResult.Succeeded("# Title", ".md");

        result.Success.Should().BeTrue();
        result.Markdown.Should().Be("# Title");
        result.SourceFormat.Should().Be(".md");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Failure_ReturnsFailureResult()
    {
        var result = ConversionResult.Failure("Something went wrong", ".pdf");

        result.Success.Should().BeFalse();
        result.Markdown.Should().BeEmpty();
        result.SourceFormat.Should().Be(".pdf");
        result.ErrorMessage.Should().Be("Something went wrong");
    }
}
