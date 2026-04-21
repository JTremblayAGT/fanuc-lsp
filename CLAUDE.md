# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test

```bash
dotnet build FanucLSP.sln
dotnet test
dotnet test --filter "FullyQualifiedName~KarelParser"   # single test project
dotnet run --project FanucLSP                           # run the LSP server
```

The `UnderAutomation.Fanuc` NuGet package requires a paid license and is not included in the repo — the build will fail if that package reference is active and unlicensed.

## Project Layout

| Project | Role |
|---|---|
| `FanucLSP/` | LSP server executable — JSON-RPC wiring, request dispatch, state, feature providers |
| `KarelParser/` | Parser for Karel (`.kl`) programs |
| `TPLangParser/` | Parser for TP (Teach Pendant) programs |
| `ParserUtils/` | Shared parser helpers (position tracking, common combinators) |
| `Tests/KarelParser.Tests/` | xUnit tests for Karel parser |
| `Tests/TPLangParser.Tests/` | xUnit tests for TP parser (theory-heavy, inline data) |

## Architecture

### Request flow

```
IDE ──LSP/JSON-RPC──► LspServer.cs (routes by method name)
                           │
                    LspServerState        ← holds open documents + parsed programs
                           │
              ┌────────────┴────────────┐
          KarelParser             TPLangParser
              │                        │
        KarelProgram             TpProgram
              └────────────┬────────────┘
                    Feature Providers
              (Completion / Definition / Hover)
```

- `Program.cs` sets up stdio transport; `JsonRPC.cs` handles framing; `LspServer.cs` dispatches to handlers.
- `LspServerState` owns all open `TextDocumentState` records and the provider lists.
- File extension `.kl` → Karel path; everything else → TP path.

### Parser combinators

Both parsers are built with **Sprache** (functional parser combinators). Parsers are composed from small, named primitives. Position tracking (line/column) is threaded through for LSP range support.

### Provider pattern

Each LSP capability is split into one or more small providers implementing a shared interface:

```
ICompletionProvider / IKlCompletionProvider
IDefinitionProvider / IKarelDefinitionProvider
IHoverProvider / IKlHoverProvider
IFormatter
```

To add a new feature, implement the relevant interface and register it in `LspServerState`.

### Embedded resources

`FanucLSP/Resources/Karel/karelbuiltin.code-snippets` is embedded into the executable at build time and loaded at runtime for Karel builtin hover/completion.

## Code Style

- Use `var` for everything.
- Prefer expression-bodied members over block-bodied members.
- Prefer switch expressions over if-else chains.
- Prefer `switch` over `if` chains.
- ALWAYS use braces for blocks, even if they're only one line.
