<!-- AI Skill: Describes how markitdown can be used as an MCP tool -->

# MarkItDotNet MCP Integration

**Model Context Protocol (MCP) integration for exposing document conversion as serverless AI tools.**

This skill teaches how to set up MarkItDotNet as an MCP tool server, enabling Claude, Copilot, Cursor, and other MCP-compatible AI agents to invoke document conversions from within conversations.

## Overview

The MarkItDotNet CLI can be wrapped as an MCP tool server, exposing document conversion capabilities to AI agents. This allows AI to:

- Convert files to Markdown within conversation flows
- Batch process documents on behalf of users
- Fetch and convert web pages to Markdown
- List and explore supported formats
- Return structured metadata alongside converted content

MCP tools are ideal for **agentic workflows** where AI needs to autonomously handle file conversions as part of larger tasks.

## MCP Tool Definitions

### Tool: `markitdown_convert`

Convert a single file to Markdown.

**Signature:**
```json
{
  "name": "markitdown_convert",
  "description": "Convert a file to Markdown format",
  "inputSchema": {
    "type": "object",
    "properties": {
      "filePath": {
        "type": "string",
        "description": "Path to the file to convert (absolute or relative)"
      },
      "format": {
        "type": "string",
        "enum": ["markdown", "json"],
        "description": "Output format: 'markdown' for plain Markdown, 'json' for structured output with metadata"
      },
      "streaming": {
        "type": "boolean",
        "description": "If true, stream large files chunk-by-chunk for memory efficiency (PDFs only)"
      }
    },
    "required": ["filePath"]
  }
}
```

**Output:**
```json
{
  "success": true,
  "markdown": "# Document Title\n\nContent...",
  "metadata": {
    "wordCount": 1250,
    "lineCount": 42,
    "processingTimeMs": 125,
    "format": ".pdf",
    "title": "Document Title"
  }
}
```

**Error Output:**
```json
{
  "success": false,
  "markdown": "",
  "error": "File not found: missing.pdf",
  "errorCode": 2
}
```

### Tool: `markitdown_batch`

Convert multiple files in a directory.

**Signature:**
```json
{
  "name": "markitdown_batch",
  "description": "Batch convert files in a directory",
  "inputSchema": {
    "type": "object",
    "properties": {
      "directory": {
        "type": "string",
        "description": "Directory containing files to convert"
      },
      "outputDirectory": {
        "type": "string",
        "description": "Output directory for converted .md files"
      },
      "pattern": {
        "type": "string",
        "description": "Glob pattern (e.g., '*.pdf', '*.{docx,pdf}'). Default: '*.*'"
      },
      "recursive": {
        "type": "boolean",
        "description": "Include subdirectories (default: false)"
      },
      "parallel": {
        "type": "integer",
        "description": "Number of parallel conversions (default: CPU cores)"
      },
      "format": {
        "type": "string",
        "enum": ["markdown", "json"],
        "description": "Output format"
      }
    },
    "required": ["directory", "outputDirectory"]
  }
}
```

**Output:**
```json
{
  "success": true,
  "filesConverted": 15,
  "filesFailed": 1,
  "processingTimeMs": 3250,
  "results": [
    {
      "file": "report.pdf",
      "success": true,
      "outputPath": "output/report.md",
      "wordCount": 2500
    },
    {
      "file": "image.png",
      "success": false,
      "error": "Unsupported format: .png"
    }
  ]
}
```

### Tool: `markitdown_url`

Convert a web page to Markdown.

**Signature:**
```json
{
  "name": "markitdown_url",
  "description": "Convert a web page to Markdown",
  "inputSchema": {
    "type": "object",
    "properties": {
      "url": {
        "type": "string",
        "description": "URL to convert (http:// or https://)"
      },
      "format": {
        "type": "string",
        "enum": ["markdown", "json"],
        "description": "Output format"
      }
    },
    "required": ["url"]
  }
}
```

**Output:**
```json
{
  "success": true,
  "markdown": "# Example Domain\n\nThis domain is...",
  "metadata": {
    "title": "Example Domain",
    "url": "https://example.com",
    "wordCount": 450,
    "fetchTimeMs": 850,
    "processingTimeMs": 75
  }
}
```

### Tool: `markitdown_formats`

List all supported file formats.

**Signature:**
```json
{
  "name": "markitdown_formats",
  "description": "List all supported file formats and converters",
  "inputSchema": {
    "type": "object",
    "properties": {}
  }
}
```

**Output:**
```json
{
  "formats": [
    {
      "name": "PDF",
      "extensions": [".pdf"],
      "converter": "PdfConverter",
      "package": "Core",
      "notes": "Text extraction + optional streaming"
    },
    {
      "name": "Word (DOCX)",
      "extensions": [".docx"],
      "converter": "DocxConverter",
      "package": "Core",
      "notes": "Headings, tables, lists, images"
    },
    {
      "name": "Excel (XLSX)",
      "extensions": [".xlsx"],
      "converter": "ExcelConverter",
      "package": "Excel",
      "notes": "Tables for each sheet"
    }
  ],
  "totalFormats": 17,
  "coreFormats": 12,
  "extendedFormats": 5
}
```

## Implementation Guide

### Setup as Node.js MCP Server

Install the MCP Node SDK and wrap the CLI:

```javascript
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { CallToolRequestSchema, TextContent, Tool } from "@modelcontextprotocol/sdk/types.js";
import { execSync, exec } from "child_process";
import { promisify } from "util";

const asyncExec = promisify(exec);

const server = new Server({
  name: "markitdown-server",
  version: "1.0.0",
});

// Tool definitions
const tools: Tool[] = [
  {
    name: "markitdown_convert",
    description: "Convert a file to Markdown format",
    inputSchema: {
      type: "object",
      properties: {
        filePath: { type: "string" },
        format: { type: "string", enum: ["markdown", "json"] },
        streaming: { type: "boolean" }
      },
      required: ["filePath"]
    }
  },
  {
    name: "markitdown_batch",
    description: "Batch convert files in a directory",
    inputSchema: {
      type: "object",
      properties: {
        directory: { type: "string" },
        outputDirectory: { type: "string" },
        pattern: { type: "string" },
        recursive: { type: "boolean" },
        parallel: { type: "integer" },
        format: { type: "string", enum: ["markdown", "json"] }
      },
      required: ["directory", "outputDirectory"]
    }
  },
  {
    name: "markitdown_url",
    description: "Convert a web page to Markdown",
    inputSchema: {
      type: "object",
      properties: {
        url: { type: "string" },
        format: { type: "string", enum: ["markdown", "json"] }
      },
      required: ["url"]
    }
  },
  {
    name: "markitdown_formats",
    description: "List all supported file formats",
    inputSchema: { type: "object", properties: {} }
  }
];

server.setRequestHandler(ListToolsRequestSchema, async () => ({
  tools
}));

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    if (name === "markitdown_convert") {
      const cmd = `markitdown "${args.filePath}"${args.format === "json" ? " --format json" : ""}${args.streaming ? " --streaming" : ""}`;
      const { stdout } = await asyncExec(cmd);
      return {
        content: [{ type: "text" as const, text: stdout }]
      };
    }
    // ... implement other tools
  } catch (error) {
    return {
      content: [{ type: "text" as const, text: `Error: ${error.message}` }],
      isError: true
    };
  }
});

const transport = new StdioServerTransport();
server.connect(transport);
```

### Configure in Claude Desktop

Add to `~/.claude/claude.json`:

```json
{
  "mcpServers": {
    "markitdown": {
      "command": "node",
      "args": ["/path/to/markitdown-mcp-server.js"],
      "env": {
        "PATH": "/usr/local/bin:/usr/bin"
      }
    }
  }
}
```

Then restart Claude Desktop. The tools will appear in the Tools panel.

### Configure in GitHub Copilot

Add to your Copilot agent configuration:

```yaml
agents:
  - name: file-processor
    tools:
      - markitdown_convert
      - markitdown_batch
      - markitdown_url
      - markitdown_formats
    mcp_servers:
      - name: markitdown
        command: node
        args: ["/path/to/markitdown-mcp-server.js"]
```

### Configure in Cursor

Add to `.cursor/tools.json`:

```json
{
  "tools": [
    {
      "name": "markitdown",
      "command": "/usr/local/bin/markitdown",
      "enabled": true,
      "description": "Convert files to Markdown"
    }
  ]
}
```

## Usage Examples

### Claude Desktop Example

**User:** "Convert this PDF report to Markdown and extract key metrics"

**Claude Actions:**
1. Calls `markitdown_convert` with filePath and format="json"
2. Receives structured output with metadata and content
3. Parses JSON to extract metrics
4. Summarizes findings in response

**Example Prompt:**
```
You have access to the markitdown_convert tool. 
Convert the file at /data/financial-report.pdf and extract:
- Total revenue
- Profit margin
- Key metrics from the report
```

### Copilot Example

**Agent Task:** "Index all documentation in our docs folder"

**Agent Actions:**
1. Calls `markitdown_batch` with directory="/docs", outputDirectory="/docs-md", recursive=true
2. Monitors progress as files convert
3. Calls `markitdown_formats` to validate all formats are supported
4. Reports summary: "Converted 42 files in 3.2 seconds"

**Example Workflow:**
```
@github Batch convert all PDFs in /docs/specs to Markdown
```

### Cursor Example

**Developer:** "Help me understand this API documentation"

**Cursor Actions:**
1. Detects user has documentation file open
2. Offers to convert via markitdown tool
3. Displays converted Markdown with syntax highlighting
4. Enables AI to understand and answer questions about the docs

## Error Handling

Tools should gracefully handle:

| Error | Exit Code | MCP Response |
|-------|-----------|-------------|
| File not found | 2 | `{ "success": false, "error": "File not found", "errorCode": 2 }` |
| Unsupported format | 3 | `{ "success": false, "error": "Unsupported format", "errorCode": 3 }` |
| Conversion failed | 1 | `{ "success": false, "error": "Conversion failed: ...", "errorCode": 1 }` |
| Invalid URL | - | `{ "success": false, "error": "Invalid URL format" }` |
| Directory not found | - | `{ "success": false, "error": "Directory not found" }` |

## Performance Considerations

1. **Timeout** — Set tool timeout to 60+ seconds for large batches
2. **Memory** — Use `--streaming` for PDFs > 100 MB
3. **Parallelism** — Adjust `--parallel` based on system resources
4. **Caching** — Cache conversion results for repeated requests

## Security Notes

- Validate file paths to prevent directory traversal attacks
- Restrict to allowed directories with path normalization
- Use sandboxing for untrusted input
- Limit URL conversions to whitelisted domains if needed
- Set resource limits (max file size, timeout, memory)

## Testing MCP Tools

Test locally before deploying:

```bash
# Test CLI directly
markitdown /path/to/file.pdf

# Test with JSON output
markitdown /path/to/file.pdf --format json | jq .

# Test batch conversion
markitdown batch ./test-files -o ./output -r

# Test URL conversion
markitdown url https://example.com
```

## Deployment

### Docker Container

```dockerfile
FROM node:18-alpine

RUN dotnet tool install -g ElBruno.MarkItDotNet.Cli

COPY markitdown-mcp-server.js /app/

ENTRYPOINT ["node", "/app/markitdown-mcp-server.js"]
```

### Lambda Function

Wrap MCP server as Lambda handler for serverless deployment:

```javascript
export const handler = async (event) => {
  const result = await markitdownServer.call(event.tool, event.args);
  return result;
};
```

## Further Integration

- **Vector Databases** — Convert files to Markdown, chunk, and embed for RAG
- **LLM Pipelines** — Ingest documents through conversion before processing
- **Automation** — Batch convert documents as part of CI/CD workflows
- **Accessibility** — Convert PDFs/images to accessible Markdown format
