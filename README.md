# Instancium Lens

Local-first AI workspace for document understanding, built on Instancium.Vectra and InstantAIGate.

Lens indexes your documents on-device and answers natural-language questions with
numbered citations back to the exact source chunk. No mandatory cloud dependency:
retrieval runs in-process (Vectra), inference runs on your machine (InstantAIGate).

## Why Lens

- Understand large document collections instead of paging through them.
- Ask questions in natural language and get grounded answers.
- Verify every claim against the original source: traceability is a contract, not a promise.
- Keep confidential data local: documents, index and models never leave the device.

## How It Works

```
┌───────────────────────────────────────────┐
│  Instancium.Lens.App (MAUI + Blazor UI)   │
└─────────────────┬─────────────────────────┘
                  │ IndexFolderAsync / AskStreamingAsync
        ┌─────────┴─────────┐
        ▼                   ▼
┌──────────────────┐  ┌───────────────┐
│ Lens.Ingestion   │  │ Lens.Chat     │
│ parse→chunk→     │  │ retrieve→     │
│ embed→store      │  │ prompt→stream │
└────────┬─────────┘  └──────┬────────┘
         ▼                   ▼
┌───────────────────┐  ┌─────────────────────────────────┐
│ Instancium.Vectra │  │ Instancium.InstantAIGate.Client │
│ local vector      │  │ SignalR: embeddings             │
│ store, cosine     │  │ (qwen3-embedding-0.6b) + LLM    │
│ search            │  │ token streaming                 │
└───────────────────┘  └─────────────────────────────────┘
```

- Retrieval: `IVectraCollection.SearchTextAsync` over a per-profile collection (Cosine metric).
- Inference: InstantAIGate hub over SignalR; the gateway must report `Ready` before use.
- Streaming: `IChatService.AskStreamingAsync` reports pipeline stages (Retrieving → Generating)
  and answer tokens; the UI never sits in an opaque "Thinking…" state.
- Citations: every answer carries `[n]` markers bound to
  `Citation(Number, Source, ChunkIndex, Text)`; the UI renders them as collapsible sources.

## Core Features (current build)

| Feature | Status |
| --- | --- |
| Recursive indexing of `.txt` / `.md` folders | Shipped |
| Live indexing progress and result summary (files, chunks) | Shipped |
| Per-folder index storage with restart-safe profile binding | Shipped |
| Grounded Q&A with stage visibility and token streaming | Shipped |
| Numbered citations with source path and chunk index | Shipped |
| PDF and Office formats | Roadmap: post-MVP, via new `IFileParser` implementations |
| Rerank, query rewriting, multi-turn context | Roadmap: Phase 5 |

## Storage Model (Local-First)

- The index lives next to the documents: `<selected root>/.instancium`
  (configurable via `Vectra:IndexFolderName`).
- Profiles never bound to a folder fall back to the default root
  `<AppData>/lens-vectra` (configurable via `Vectra:BaseDirectory`).
- The profile id is the Vectra collection name; the profile → root binding persists
  across restarts, so chat resolves the same index after relaunch.
- Enterprise mode (remote Vectra) is a configuration-driven switch of `IVectraEngine`;
  business logic (`IngestionService`, `ChatService`) stays unchanged.

## Quick Start

Prerequisites:

- Windows 10 1809+ (build 17763); the app targets `net10.0-windows10.0.19041.0`.
- .NET 10 SDK with the MAUI workload: `dotnet workload install maui`.
- A running InstantAIGate instance with `qwen3-embedding-0.6b` (1024 dimensions)
  and a chat model loaded; gateway status must be `Ready`.

Run:

1. Provide the gateway key via the environment variable `Gateway__ApiKey`
   (the bundled `appsettings.json` carries a placeholder; real keys never enter the repository).
2. `dotnet build InstanciumLens.slnx`, then run the `Instancium.Lens.App` project.
3. In the app: pick a folder → Index → switch to Chat → ask a question.

Test:

- `dotnet test InstanciumLens.slnx` runs the xUnit v3 suites for Core and Chat.

## Configuration

Bundled `Resources/Raw/appsettings.json`, overridable by environment variables:

| Key | Default | Meaning |
| --- | --- | --- |
| `Gateway:BaseUrl` | `http://localhost:5000` | InstantAIGate base URL |
| `Gateway:HubPath` | `/hub/gateway` | SignalR hub path |
| `Gateway:ApiKey` | placeholder | Auth key; override via `Gateway__ApiKey` |
| `Gateway:EmbeddingRepoId` | `qwen3-embedding-0.6b` | Embedding model id |
| `Gateway:ExpectedEmbeddingDimension` | `1024` | Validated on first embed call |
| `Gateway:ContextPolicy` | `Stateless` | Session retention: `Stateless` or `Conversational` |
| `Vectra:BaseDirectory` | empty → AppData | Default index root for unbound profiles |
| `Vectra:IndexFolderName` | `.instancium` | Index subfolder inside the scanned root |
| `Chat:TopK` | `5` | Context chunks retrieved per question |
| `Chunking:ChunkSize` / `Chunking:Overlap` | `512` / `50` | Chunker parameters |


## License & Trademark

Copyright (c) 2026 Instancium™ (https://instancium.com). All rights reserved.

- Source code: Apache License 2.0 — see [LICENSE](LICENSE.txt).
- Third-party components, models and licenses: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
- Branding: the Instancium and Instancium Lens names, logos and icon assets
  (`Resources/AppIcon`, `Resources/Splash`, `wwwroot/imgs`) are the exclusive property
  of Instancium, licensed under [CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/);
  reference use only, no modification, no commercial implication.

