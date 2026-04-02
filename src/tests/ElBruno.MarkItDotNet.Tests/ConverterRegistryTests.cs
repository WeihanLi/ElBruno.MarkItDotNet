using ElBruno.MarkItDotNet.Converters;
using FluentAssertions;
using Xunit;

namespace ElBruno.MarkItDotNet.Tests;

public class ConverterRegistryTests
{
    [Fact]
    public void Register_AddsConverterToRegistry()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());
        registry.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void Register_NullConverter_ThrowsArgumentNullException()
    {
        var registry = new ConverterRegistry();
        var act = () => registry.Register(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Resolve_ReturnsMatchingConverter()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());

        var converter = registry.Resolve(".txt");

        converter.Should().NotBeNull();
        converter.Should().BeOfType<PlainTextConverter>();
    }

    [Fact]
    public void Resolve_UnknownExtension_ReturnsNull()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());

        var converter = registry.Resolve(".xyz");

        converter.Should().BeNull();
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());

        registry.Resolve(".TXT").Should().NotBeNull();
        registry.Resolve(".Txt").Should().NotBeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllRegisteredConverters()
    {
        var registry = new ConverterRegistry();
        registry.Register(new PlainTextConverter());

        var all = registry.GetAll();

        all.Should().HaveCount(1);
        all[0].Should().BeOfType<PlainTextConverter>();
    }
}
