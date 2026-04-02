using System.Diagnostics;
using System.Text;
using ElBruno.MarkItDotNet;
using ElBruno.MarkItDotNet.Excel;
using ElBruno.MarkItDotNet.PowerPoint;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.MarkItDotNet - Batch Processor Sample           ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

// Parse command-line arguments for input/output directories
var inputDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "input");
var outputDir = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "output");

Console.WriteLine($"📂 Input directory:  {inputDir}");
Console.WriteLine($"📂 Output directory: {outputDir}\n");

// Ensure directories exist
Directory.CreateDirectory(inputDir);
Directory.CreateDirectory(outputDir);

// Create sample files in the input directory if it's empty
if (Directory.GetFiles(inputDir).Length == 0)
{
    Console.WriteLine("📝 No files found in input directory. Creating sample files...\n");
    CreateSampleFiles(inputDir);
}

// Set up DI container with core + Excel + PowerPoint converters
var services = new ServiceCollection();
services.AddMarkItDotNet();
services.AddMarkItDotNetExcel();
services.AddMarkItDotNetPowerPoint();
var serviceProvider = services.BuildServiceProvider();

var markdownService = serviceProvider.GetRequiredService<MarkdownService>();

// Scan input directory for all files
var files = Directory.GetFiles(inputDir);
Console.WriteLine($"🔍 Found {files.Length} file(s) to process.\n");
Console.WriteLine("─────────────────────────────────────────────────────────");

var results = new List<BatchResult>();
var totalStopwatch = Stopwatch.StartNew();

foreach (var filePath in files)
{
    var fileName = Path.GetFileName(filePath);
    var extension = Path.GetExtension(filePath).ToLowerInvariant();
    var outputPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath) + ".md");
    var sw = Stopwatch.StartNew();

    try
    {
        // Use streaming for PDF files to demonstrate the streaming API
        if (extension == ".pdf")
        {
            var markdownBuilder = new StringBuilder();
            await foreach (var chunk in markdownService.ConvertStreamingAsync(filePath))
            {
                markdownBuilder.Append(chunk);
            }

            sw.Stop();
            var markdown = markdownBuilder.ToString();
            var wordCount = CountWords(markdown);

            await File.WriteAllTextAsync(outputPath, markdown);

            results.Add(new BatchResult(fileName, extension, true, wordCount, sw.Elapsed, null));
        }
        else
        {
            var result = await markdownService.ConvertAsync(filePath);
            sw.Stop();

            if (result.Success)
            {
                await File.WriteAllTextAsync(outputPath, result.Markdown);

                results.Add(new BatchResult(
                    fileName, extension, true,
                    result.Metadata?.WordCount ?? 0,
                    result.Metadata?.ProcessingTime ?? sw.Elapsed,
                    null));
            }
            else
            {
                results.Add(new BatchResult(fileName, extension, false, 0, sw.Elapsed, result.ErrorMessage));
            }
        }
    }
    catch (Exception ex)
    {
        sw.Stop();
        results.Add(new BatchResult(fileName, extension, false, 0, sw.Elapsed, ex.Message));
    }
}

totalStopwatch.Stop();

// Print summary table
Console.WriteLine("\n📊 BATCH PROCESSING RESULTS");
Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
Console.WriteLine($"{"File",-30} {"Format",-10} {"Status",-10} {"Words",8} {"Time",12}");
Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");

foreach (var r in results)
{
    var status = r.Success ? "✅ OK" : "❌ FAIL";
    var words = r.Success ? r.WordCount.ToString() : "-";
    var time = $"{r.ProcessingTime.TotalMilliseconds:F1}ms";
    Console.WriteLine($"{Truncate(r.FileName, 30),-30} {r.Format,-10} {status,-10} {words,8} {time,12}");

    if (!r.Success && r.Error is not null)
    {
        Console.WriteLine($"  └─ Error: {r.Error}");
    }
}

Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");

var succeeded = results.Count(r => r.Success);
var failed = results.Count(r => !r.Success);
var totalWords = results.Where(r => r.Success).Sum(r => r.WordCount);

Console.WriteLine($"\n📈 Summary: {succeeded} succeeded, {failed} failed, {totalWords} total words");
Console.WriteLine($"⏱  Total processing time: {totalStopwatch.Elapsed.TotalMilliseconds:F1}ms");
Console.WriteLine($"📂 Output written to: {outputDir}\n");

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║             Batch Processing Complete!                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

// --- Helper methods ---

static void CreateSampleFiles(string directory)
{
    File.WriteAllText(Path.Combine(directory, "readme.txt"), """
        Welcome to MarkItDotNet!
        
        This library converts various file formats to Markdown.
        It supports plain text, HTML, JSON, CSV, Excel, PowerPoint, and more.
        
        Use cases:
        - AI/RAG pipeline ingestion
        - Documentation generation
        - Content migration
        - Search indexing
        """);

    File.WriteAllText(Path.Combine(directory, "data.csv"), """
        Name,Role,Language
        Alice,Backend,C#
        Bob,Frontend,TypeScript
        Charlie,DevOps,Python
        Diana,Data Science,R
        """);

    File.WriteAllText(Path.Combine(directory, "config.json"), """
        {
          "appName": "BatchProcessor",
          "version": "1.0.0",
          "settings": {
            "maxRetries": 3,
            "timeout": 30,
            "formats": ["txt", "csv", "json", "html"]
          },
          "database": {
            "host": "localhost",
            "port": 5432,
            "name": "markitdown_db"
          }
        }
        """);

    File.WriteAllText(Path.Combine(directory, "report.html"), """
        <!DOCTYPE html>
        <html>
        <head><title>Monthly Report</title></head>
        <body>
            <h1>Monthly Status Report</h1>
            <h2>Project Highlights</h2>
            <ul>
                <li>Completed the new conversion pipeline</li>
                <li>Added support for 3 new file formats</li>
                <li>Improved processing speed by 40%</li>
            </ul>
            <h2>Metrics</h2>
            <table>
                <tr><th>Metric</th><th>Value</th></tr>
                <tr><td>Files Processed</td><td>1,234</td></tr>
                <tr><td>Avg Processing Time</td><td>45ms</td></tr>
                <tr><td>Success Rate</td><td>99.8%</td></tr>
            </table>
            <p>Next steps: integrate with the <strong>RAG pipeline</strong> for AI-powered search.</p>
        </body>
        </html>
        """);

    Console.WriteLine("   ✅ Created readme.txt");
    Console.WriteLine("   ✅ Created data.csv");
    Console.WriteLine("   ✅ Created config.json");
    Console.WriteLine("   ✅ Created report.html\n");
}

static string Truncate(string value, int maxLength) =>
    value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";

static int CountWords(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return 0;
    var count = 0;
    var inWord = false;
    foreach (var ch in text)
    {
        if (char.IsWhiteSpace(ch)) { inWord = false; }
        else if (!inWord) { inWord = true; count++; }
    }
    return count;
}

// --- Result record ---

record BatchResult(string FileName, string Format, bool Success, int WordCount, TimeSpan ProcessingTime, string? Error);
