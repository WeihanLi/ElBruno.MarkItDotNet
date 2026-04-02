using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ElBruno.MarkItDotNet;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ElBruno.MarkItDotNet - RAG Ingestion Pipeline Sample    ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

// ============================================================================
// Step 1: Set up DI and MarkdownService
// In a real RAG pipeline, this would be part of your application startup.
// ============================================================================
var services = new ServiceCollection();
services.AddMarkItDotNet();
var serviceProvider = services.BuildServiceProvider();
var markdownService = serviceProvider.GetRequiredService<MarkdownService>();

// ============================================================================
// Step 2: Define sample documents
// These simulate documents you'd ingest from a file system, database, or API.
// Each document has an ID, a format, and raw content.
// ============================================================================
Console.WriteLine("📄 Step 1: Preparing sample documents for ingestion...\n");

var documents = new List<SourceDocument>
{
    new("doc-001", ".txt", """
        Retrieval-Augmented Generation (RAG) is a technique that enhances large language models
        by retrieving relevant documents from a knowledge base before generating a response.

        The RAG pipeline typically consists of three stages:
        1. Document ingestion - converting raw documents into a searchable format
        2. Retrieval - finding relevant documents based on a user query
        3. Generation - using the retrieved context to generate an accurate response

        Benefits of RAG include reduced hallucination, up-to-date knowledge, and the ability
        to cite sources. RAG is particularly useful for enterprise applications where accuracy
        and traceability are critical.

        Common document formats in RAG pipelines include PDF, HTML, DOCX, and plain text.
        A good ingestion pipeline must handle all these formats uniformly.
        """),

    new("doc-002", ".html", """
        <!DOCTYPE html>
        <html>
        <body>
            <h1>Vector Embeddings for Search</h1>
            <p>Vector embeddings are numerical representations of text that capture semantic meaning.
            They are the foundation of modern semantic search systems.</p>

            <h2>How Embeddings Work</h2>
            <p>Text is passed through a neural network (embedding model) that produces a fixed-size
            vector. Similar texts produce vectors that are close together in the embedding space.</p>

            <h2>Popular Embedding Models</h2>
            <ul>
                <li><strong>OpenAI text-embedding-ada-002</strong> - 1536 dimensions</li>
                <li><strong>Cohere embed-v3</strong> - 1024 dimensions</li>
                <li><strong>sentence-transformers</strong> - Various sizes, open source</li>
            </ul>

            <h2>Chunking Strategy</h2>
            <p>Before creating embeddings, documents must be split into chunks. Chunk size affects
            retrieval quality: too large and you lose precision, too small and you lose context.
            A typical chunk size is 200-500 tokens with some overlap between chunks.</p>
        </body>
        </html>
        """),

    new("doc-003", ".json", """
        {
          "title": "MarkItDotNet Integration Guide",
          "author": "Bruno Capuano",
          "sections": [
            {
              "heading": "Overview",
              "content": "MarkItDotNet is a .NET library for converting documents to Markdown, ideal for RAG pipelines."
            },
            {
              "heading": "Supported Formats",
              "content": "The library supports TXT, HTML, JSON, CSV, PDF, DOCX, XLSX, PPTX, RTF, EPUB, and images."
            },
            {
              "heading": "Getting Started",
              "content": "Install via NuGet: dotnet add package ElBruno.MarkItDotNet. Register services with AddMarkItDotNet()."
            }
          ]
        }
        """)
};

Console.WriteLine($"   ✅ {documents.Count} documents prepared\n");

// ============================================================================
// Step 3: Convert each document to Markdown using MarkdownService
// This normalizes all formats into a consistent Markdown representation.
// ============================================================================
Console.WriteLine("🔄 Step 2: Converting documents to Markdown...\n");

var convertedDocuments = new List<ConvertedDocument>();

foreach (var doc in documents)
{
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(doc.Content));
    var result = await markdownService.ConvertAsync(stream, doc.Format);

    if (result.Success)
    {
        convertedDocuments.Add(new ConvertedDocument(doc.Id, doc.Format, result.Markdown, result.Metadata));
        Console.WriteLine($"   ✅ {doc.Id} ({doc.Format}) → {result.Metadata?.WordCount ?? 0} words");
    }
    else
    {
        Console.WriteLine($"   ❌ {doc.Id} ({doc.Format}) → Error: {result.ErrorMessage}");
    }
}

Console.WriteLine();

// ============================================================================
// Step 4: Chunk the Markdown into segments for embedding
// Simple paragraph-based chunking: split on double newlines, merge small
// paragraphs, and enforce a max chunk size of 500 characters.
// In production, you'd use more sophisticated strategies (sliding window,
// sentence-aware splitting, semantic chunking).
// ============================================================================
Console.WriteLine("✂️  Step 3: Chunking Markdown into segments...\n");

const int maxChunkSize = 500;
var allChunks = new List<DocumentChunk>();

foreach (var doc in convertedDocuments)
{
    var chunks = ChunkMarkdown(doc.Markdown, maxChunkSize);

    for (var i = 0; i < chunks.Count; i++)
    {
        allChunks.Add(new DocumentChunk(
            DocumentId: doc.Id,
            ChunkIndex: i,
            Content: chunks[i],
            SourceFormat: doc.Format,
            WordCount: CountWords(chunks[i]),
            CharCount: chunks[i].Length));
    }

    Console.WriteLine($"   📑 {doc.Id}: {chunks.Count} chunk(s)");
}

Console.WriteLine();

// ============================================================================
// Step 5: Output structured JSON for each chunk
// This JSON structure is ready to be sent to an embedding API and stored
// in a vector database (e.g., Azure AI Search, Pinecone, Qdrant, Weaviate).
// ============================================================================
Console.WriteLine("📤 Step 4: Generating structured JSON output...\n");

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var jsonOutput = JsonSerializer.Serialize(allChunks, jsonOptions);
Console.WriteLine(jsonOutput);

// ============================================================================
// Step 6: Print pipeline statistics
// ============================================================================
Console.WriteLine("\n═══════════════════════════════════════════════════════════");
Console.WriteLine("📊 RAG PIPELINE STATISTICS");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine($"   Documents processed:  {convertedDocuments.Count}");
Console.WriteLine($"   Total chunks:         {allChunks.Count}");
Console.WriteLine($"   Total words:          {allChunks.Sum(c => c.WordCount)}");
Console.WriteLine($"   Total characters:     {allChunks.Sum(c => c.CharCount)}");
Console.WriteLine($"   Avg chunk size:       {(allChunks.Count > 0 ? allChunks.Average(c => c.CharCount) : 0):F0} chars");
Console.WriteLine("═══════════════════════════════════════════════════════════\n");

Console.WriteLine("💡 Next steps in a real RAG pipeline:");
Console.WriteLine("   1. Send each chunk to an embedding model (e.g., OpenAI, Cohere)");
Console.WriteLine("   2. Store embeddings + metadata in a vector database");
Console.WriteLine("   3. At query time, embed the user question");
Console.WriteLine("   4. Retrieve top-K similar chunks");
Console.WriteLine("   5. Pass retrieved chunks as context to an LLM\n");

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║          RAG Pipeline Sample Complete!                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

// --- Helper methods ---

/// <summary>
/// Splits Markdown text into chunks by paragraph boundaries (double newlines),
/// merging small paragraphs and splitting oversized ones.
/// </summary>
static List<string> ChunkMarkdown(string markdown, int maxChunkSize)
{
    var chunks = new List<string>();
    var paragraphs = markdown.Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries);
    var currentChunk = new StringBuilder();

    foreach (var paragraph in paragraphs)
    {
        var trimmed = paragraph.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            continue;

        // If adding this paragraph would exceed max size, finalize the current chunk
        if (currentChunk.Length > 0 && currentChunk.Length + trimmed.Length + 2 > maxChunkSize)
        {
            chunks.Add(currentChunk.ToString().Trim());
            currentChunk.Clear();
        }

        // If a single paragraph exceeds max size, split it
        if (trimmed.Length > maxChunkSize)
        {
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }

            // Split long paragraph at sentence boundaries or word boundaries
            var remaining = trimmed;
            while (remaining.Length > maxChunkSize)
            {
                var splitIndex = remaining.LastIndexOf(". ", maxChunkSize);
                if (splitIndex <= 0) splitIndex = remaining.LastIndexOf(' ', maxChunkSize);
                if (splitIndex <= 0) splitIndex = maxChunkSize;

                chunks.Add(remaining[..splitIndex].Trim());
                remaining = remaining[splitIndex..].Trim();
            }

            if (remaining.Length > 0)
                currentChunk.Append(remaining);
        }
        else
        {
            if (currentChunk.Length > 0) currentChunk.Append("\n\n");
            currentChunk.Append(trimmed);
        }
    }

    if (currentChunk.Length > 0)
        chunks.Add(currentChunk.ToString().Trim());

    return chunks;
}

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

// --- Records ---

record SourceDocument(string Id, string Format, string Content);
record ConvertedDocument(string Id, string Format, string Markdown, ConversionMetadata? Metadata);
record DocumentChunk(string DocumentId, int ChunkIndex, string Content, string SourceFormat, int WordCount, int CharCount);
