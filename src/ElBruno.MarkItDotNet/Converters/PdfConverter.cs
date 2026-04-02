using System.Runtime.CompilerServices;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ElBruno.MarkItDotNet.Converters;

/// <summary>
/// Converts PDF (.pdf) files to Markdown using UglyToad.PdfPig.
/// Uses word-level extraction with line/paragraph grouping and heading detection.
/// Implements <see cref="IStreamingMarkdownConverter"/> for page-by-page streaming.
/// </summary>
public class PdfConverter : IStreamingMarkdownConverter
{
    private const double LineYTolerance = 3.0;
    private const double ParagraphSpacingFactor = 1.5;
    private const double HeadingFontSizeRatio = 1.2;

    /// <inheritdoc />
    public bool CanHandle(string fileExtension) =>
        fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<string> ConvertAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using var document = PdfDocument.Open(fileStream);
        var sb = new StringBuilder();
        var pageCount = document.NumberOfPages;

        for (var i = 1; i <= pageCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = document.GetPage(i);

            if (i > 1)
            {
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            var pageMarkdown = ExtractPageMarkdown(page);
            if (!string.IsNullOrWhiteSpace(pageMarkdown))
            {
                sb.AppendLine(pageMarkdown.TrimEnd());
            }
        }

        return Task.FromResult(sb.ToString().TrimEnd());
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ConvertStreamingAsync(
        Stream fileStream,
        string fileExtension,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using var document = PdfDocument.Open(fileStream);
        var pageCount = document.NumberOfPages;

        for (var i = 1; i <= pageCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = document.GetPage(i);
            var chunkSb = new StringBuilder();

            if (i > 1)
            {
                chunkSb.AppendLine();
                chunkSb.AppendLine("---");
                chunkSb.AppendLine();
            }

            var pageMarkdown = ExtractPageMarkdown(page);
            if (!string.IsNullOrWhiteSpace(pageMarkdown))
            {
                chunkSb.AppendLine(pageMarkdown.TrimEnd());
            }

            yield return chunkSb.ToString();
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Extracts structured markdown from a single page using word-level extraction.
    /// Groups words into lines by Y-coordinate, lines into paragraphs by spacing,
    /// and detects headings by font size.
    /// </summary>
    private static string ExtractPageMarkdown(Page page)
    {
        var words = page.GetWords().ToList();
        if (words.Count == 0)
        {
            // Fallback to raw text if word extraction yields nothing
            var rawText = page.Text;
            return string.IsNullOrWhiteSpace(rawText) ? string.Empty : rawText.Trim();
        }

        var lines = GroupWordsIntoLines(words);
        if (lines.Count == 0)
            return string.Empty;

        var bodyFontSize = DetectBodyFontSize(lines);
        var paragraphs = GroupLinesIntoParagraphs(lines, bodyFontSize);

        var sb = new StringBuilder();
        foreach (var paragraph in paragraphs)
        {
            var text = paragraph.Text;
            if (string.IsNullOrWhiteSpace(text))
                continue;

            if (paragraph.IsHeading && bodyFontSize > 0)
            {
                var ratio = paragraph.FontSize / bodyFontSize;
                var level = ratio >= 1.8 ? 1 : ratio >= 1.4 ? 2 : 3;
                sb.Append(new string('#', level));
                sb.Append(' ');
            }

            sb.AppendLine(text);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static List<TextLine> GroupWordsIntoLines(List<Word> words)
    {
        // Sort words by Y descending (top of page first), then X ascending
        var sorted = words
            .OrderByDescending(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left)
            .ToList();

        var lines = new List<TextLine>();
        var currentLineWords = new List<Word>();
        var currentY = double.MaxValue;

        foreach (var word in sorted)
        {
            var wordY = word.BoundingBox.Bottom;

            if (currentLineWords.Count == 0 || Math.Abs(wordY - currentY) <= LineYTolerance)
            {
                currentLineWords.Add(word);
                if (currentLineWords.Count == 1)
                    currentY = wordY;
                else
                    currentY = currentLineWords.Average(w => w.BoundingBox.Bottom);
            }
            else
            {
                lines.Add(CreateTextLine(currentLineWords));
                currentLineWords = [word];
                currentY = wordY;
            }
        }

        if (currentLineWords.Count > 0)
            lines.Add(CreateTextLine(currentLineWords));

        return lines;
    }

    private static TextLine CreateTextLine(List<Word> words)
    {
        var sorted = words.OrderBy(w => w.BoundingBox.Left).ToList();
        var text = string.Join(" ", sorted.Select(w => w.Text));
        var avgFontSize = sorted
            .SelectMany(w => w.Letters)
            .Select(l => l.PointSize)
            .DefaultIfEmpty(0)
            .Average();
        var y = sorted.Average(w => w.BoundingBox.Bottom);
        var height = sorted.Max(w => w.BoundingBox.Height);

        return new TextLine(text, y, height, avgFontSize);
    }

    /// <summary>
    /// Detects the most common (body) font size across all lines.
    /// </summary>
    private static double DetectBodyFontSize(List<TextLine> lines)
    {
        var fontSizes = lines
            .Where(l => l.FontSize > 0)
            .GroupBy(l => Math.Round(l.FontSize, 1))
            .OrderByDescending(g => g.Sum(l => l.Text.Length))
            .ToList();

        return fontSizes.Count > 0 ? fontSizes[0].Key : 0;
    }

    private static List<ParagraphInfo> GroupLinesIntoParagraphs(List<TextLine> lines, double bodyFontSize)
    {
        var paragraphs = new List<ParagraphInfo>();
        if (lines.Count == 0)
            return paragraphs;

        var currentLines = new List<TextLine> { lines[0] };

        for (var i = 1; i < lines.Count; i++)
        {
            var prevLine = lines[i - 1];
            var currentLine = lines[i];

            // Lines are ordered top-to-bottom (Y descending), so spacing = prev.Y - current.Y
            var spacing = prevLine.Y - currentLine.Y;
            var avgHeight = (prevLine.Height + currentLine.Height) / 2;
            var isNewParagraph = avgHeight > 0 && spacing > avgHeight * ParagraphSpacingFactor;

            // Also break paragraph if font size changes significantly
            var fontSizeChanged = Math.Abs(prevLine.FontSize - currentLine.FontSize) > 1.0;

            if (isNewParagraph || fontSizeChanged)
            {
                paragraphs.Add(CreateParagraph(currentLines, bodyFontSize));
                currentLines = [currentLine];
            }
            else
            {
                currentLines.Add(currentLine);
            }
        }

        if (currentLines.Count > 0)
            paragraphs.Add(CreateParagraph(currentLines, bodyFontSize));

        return paragraphs;
    }

    private static ParagraphInfo CreateParagraph(List<TextLine> lines, double bodyFontSize)
    {
        var text = string.Join(" ", lines.Select(l => l.Text));
        var avgFontSize = lines.Average(l => l.FontSize);
        var isHeading = bodyFontSize > 0 &&
                        avgFontSize > bodyFontSize * HeadingFontSizeRatio &&
                        lines.Count <= 3;

        return new ParagraphInfo(text, avgFontSize, isHeading);
    }

    private sealed record TextLine(string Text, double Y, double Height, double FontSize);
    private sealed record ParagraphInfo(string Text, double FontSize, bool IsHeading);
}
