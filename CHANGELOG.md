# Changelog

All notable changes to the Instancium Lens solution are documented in this file.
The format follows Keep a Changelog; versioning follows SemVer.

## [1.2.0] - 2026-11-06

Clean-slate rebuild baseline. Core RAG pipeline libraries
(parse → chunk → embed → index → retrieve → answer) implemented against
the real Instancium.Vectra 1.1.1 public API and the InstantAIGate Guide v1.5
hub contract. Composition root (Instancium.Lens.App) and UI are confirmed
NOT implemented as of this snapshot.

### Intentional deviations from the technical baseline

- Product version is **1.2.0**; `Instancium.Lens.Core` carries `1.2.0-rc.2`,
  remaining modules carry `0.1.0` (version alignment pending).
- `InstantAIGate.Core` pinned to **1.5.3** (Guide v1.5 references 1.5.5;
  server actually ships 1.5.5).
- `Instancium.Vectra` pinned to **1.1.1** (Vectra Integration Guide references
  `Instancium.Vectra.Sdk` 1.1.0-rc.6); package name is `Instancium.Vectra`,
  not `Instancium.Vectra.Sdk`.

### Added

**Instancium.Lens.Core**
- `Result` / `Result<TValue>` value-based failure model; failures travel as
  values, not exceptions.
- `LensError` with stable machine-readable codes (`<area>.<reason>`).
- `ChunkPayload` — versioned vector payload schema v1 (`v`, `text`, `source`,
  `chunkIndex`); additive-only evolution rule; readers tolerate unknown fields.

**Instancium.FileParser**
- `IFileParser` contract (`CanParse` / `ParseAsync`).
- `TextFileParser` (.txt) and `MarkdownFileParser` (.md) with line-ending and
  whitespace normalization; markdown title metadata extraction.

**Instancium.Chunker**
- `IChunker` contract; `TextChunker` fixed-size chunking with configurable
  overlap (defaults 512 / 50) and sentence-boundary break heuristic.
- `ChunkingOptions` with validation.

**Instancium.InstantAIGate.Client**
- `GatewayConnection` (`IGatewayConnection`): SignalR hub connection with
  Bearer token auth, automatic reconnect policy, readiness gate via
  `GetGatewayStatus` before first use, `IAsyncDisposable`.
- `GatewayEmbeddingProvider`: implements the Vectra `IEmbeddingProvider` seam;
  explicit embedding `repoId`; `EmbeddingRequest(Normalize: true)`; one-time
  dimension validation via `GetEmbeddingDimensionAsync` (dimension is verified
  against the gateway, never hardcoded).
- `GatewayChatClient` (`IChatClient`): per-request session lifecycle
  (`JoinSession` → `SendPromptDelta` → aggregate `ReceiveTokenDelta` via
  channel → `LeaveSession` in `finally`); `CompleteAsync` and `StreamAsync`;
  `ReceiveError` / `ReceiveContextOverflow` surfaced as exceptions.
- `GatewayOptions`: BaseUrl, HubPath, ApiKey, EmbeddingRepoId, LlmRepoId,
  ExpectedEmbeddingDimension, ReconnectDelays; validation.

**Instancium.Lens.Ingestion**
- `IngestionService`: recursive scan of `.txt` / `.md`, per-file
  parse → chunk → embed → store pipeline.
- MVP re-index strategy: `DeleteCollectionAsync` + recreate (per plan).
- `IProgress<IndexingProgress>` reporting (files processed / chunks indexed).
- Result-based failure codes: `ingestion.folder_not_found`,
  `ingestion.cancelled`, `ingestion.failed`.

**Instancium.Lens.Chat**
- `ChatService`: storage-agnostic RAG orchestration
  (retrieve → prompt → complete → cite); default topK = 5.
- `VectraRetrievalService` behind the `IRetrievalService` seam: search goes
  through `SearchTextAsync` (preserves the query-text hook required for future
  rerankers); payloads hydrated from search hits.
- `PromptBuilder` with `[n]` citation markers; citation numbering provably
  consistent with prompt numbering (same ordered collection, test-covered).
- `QueryBuilder` pass-through seam reserved for Phase 5 history rewriting.
- Failure codes: `chat.no_context`, `chat.empty_response`, `chat.cancelled`,
  `chat.completion_failed`.

**Tests**
- `Instancium.Lens.Core.Tests`: payload contract round-trip and future-field
  tolerance, `Result` / `LensError` contracts, assembly version baseline.
- `Instancium.Lens.Chat.Tests`: orchestration (citations, topK propagation,
  failure propagation, no-context short-circuit, LLM failure / empty /
  cancellation), prompt formatting, query builder, chat options.

### Verified against real APIs (not stubs)

The integration surface in Lens was verified against the actual provided
source of the pinned packages — no fictional types were introduced.

- **Instancium.Vectra 1.1.1**: `GetOrOpenCollection(name, CollectionOptions)`,
  `CollectionOptions(dimension, metric, modelId, fingerprint, quantization)`,
  `SimilarityMetric` enum, `VectorEntry(vectorId: long?, vector, payload)`
  (engine auto-assigns id when `vectorId` is null), instance extension
  `collection.SearchTextAsync(provider, query, options)`,
  `collection.GetPayloadTextAsync(vectorId)`, `VectorSearchHit` with hydrated
  `Payload` field — all match the Lens consumption site.
- **InstantAIGate.Core 1.5.5**: `IGatewayHub` contract (JoinSession /
  LeaveSession / SendPromptDelta / GetSessionTokenCount / GetGatewayStatus /
  GenerateEmbeddingsAsync / GetEmbeddingDimensionAsync), `IGatewayHubClient`
  events (ReceiveTokenDelta / ReceiveError / ReceiveContextOverflow),
  DTOs (`SessionTokenDelta`, `ContextOverflowReport`, `EmbeddingRequest`,
  `EmbeddingResponse`, `ChatMessage`, `GatewayStatusDetails`) — all match the
  Lens client wire contract byte-for-byte.
- **Hub string names** used by Lens client (`"ReceiveTokenDelta"`,
  `"ReceiveError"`, `"ReceiveContextOverflow"`, `"JoinSession"`,
  `"LeaveSession"`, `"SendPromptDelta"`, `"GenerateEmbeddingsAsync"`,
  `"GetEmbeddingDimensionAsync"`, `"GetGatewayStatus"`) are verified against
  `InstantAIGate.Server.Hubs.GatewayHub` source.

### Not implemented

**Phase 1 gaps (exit criteria currently unmet)**
- `Instancium.Lens.App` composition root: .NET MAUI Blazor Hybrid host, DI
  wiring, Home/Chat pages, indexing progress UI, configuration artifacts
  (`config_vectra.json`, user secrets). **Confirmed absent.**
- Architecture tests (`tests/Instancium.Lens.Architecture.Tests`) enforcing
  forbidden-coupling rules (baseline §3.4 / §5.3).
- End-to-end validation with real embedding and LLM models
  (acceptance criteria 1–6). No evidence artifacts in the snapshot.

**Deferred by plan (untouched, per roadmap)**
- Phase 2: dual-mode storage / `RemoteVectraEngine`, reconnect-rejoin session
  recovery, timeouts and retries, batch embedding, per-file index state.
- Phase 3: PDF/DOCX/HTML parsers, heading-aware chunking, metadata filtering.
- Phase 4: WAL / segments / mmap; image and media recognition.
- Phase 5: UI streaming, multi-turn KV-cache reuse, context-overflow
  mitigation (`RollbackSession`), chat history persistence.
- Phase 6–8: HNSW, quantization, NuGet extraction, multi-tenancy.

### Known discrepancies against the guides

1. **Vectra Integration Guide is stale against Instancium.Vectra 1.1.1.**
   Lens code correctly consumes the real API:
   `GetOrOpenCollection` + `CollectionOptions` + `SimilarityMetric` +
   `VectorEntry` (long? vectorId) + instance extension
   `collection.SearchTextAsync` + hydrated `VectorSearchHit.Payload`.
   The guide documents obsolete names: `GetOrCreateCollectionAsync` +
   `CollectionSpec` + `Metric` + `VectraRecord` (string id) + static
   `VectraTextExtensions.SearchTextAsync` + `SearchHit` +
   `GetPayloadTextAsync(hit.Id)`. Either the guide or the Lens code is the
   source of truth — recommend updating the guide to match the shipped 1.1.1
   surface.
2. **Vector IDs are engine-assigned** (`VectorEntry(vectorId: null, ...)`),
   while the Vectra guide requires IDs stable per `(source, chunkIndex)`.
   Tolerable for MVP delete-recreate; **blocks Phase 2 per-file index state**
   (cannot diff "already-indexed" chunks across runs).
3. **Concurrency hazard in `GatewayChatClient`**: `ReceiveError` and
   `ReceiveContextOverflow` handlers are not filtered by `sessionId`;
   concurrent sessions on the shared `HubConnection` can cross-fault.
   Guide §3.2 mandates sessionId routing.
4. **Event subscriptions registered after `StartAsync`**: Guide §10 checklist
   item "All 7 events §3.2 registered before `StartAsync`" is violated —
   current code registers after `_connection.GetConnectionAsync()` (which
   internally calls `StartAsync`). Race window on first call.
5. **No operation timeouts**: `GatewayOptions` lacks inference / control
   timeouts (guide §8 prescribes 100 s / 30 s, configuration-driven).
6. **No session-not-found retry** (guide §7 prescribes one retry with session
   re-creation on "was not found" / "not active").
7. **Module version drift**: `Instancium.Lens.Core` 1.2.0-rc.2 vs 0.1.0 for
   all other modules.
8. **Test coverage gap**: no tests for Ingestion, FileParser, Chunker,
   InstantAIGate.Client; the ≥ 80% coverage KPI is not met.
9. Naming: roadmap's `GatewayLlmClient` is implemented as
   `GatewayChatClient` (naming-only deviation).
10. `Instancium.Vectra` package name mismatch with the guide
    (`Instancium.Vectra.Sdk`). The reference package in Lens is correct for
    the actual published artifact; guide should be updated.

### Recommended next actions (priority order)

1. Create `Instancium.Lens.App` composition root — unblocks MVP exit criteria.
2. Fix concurrency hazard in `GatewayChatClient` (filter event handlers by
   `sessionId`).
3. Register event subscriptions before `StartAsync` in `GatewayConnection`
   (separate the build/start lifecycle).
4. Add `InferenceTimeoutSeconds` / `ControlTimeoutSeconds` to `GatewayOptions`
   and apply via `CancellationTokenSource` on every `InvokeAsync`.
5. Implement one-shot session-not-found retry in `GatewayChatClient` with
   `JoinSession` re-creation.
6. Align module versions to `1.2.0` (or document the 0.1.0 split explicitly).
7. Add architecture tests (`NetArchTest` rules enforcing §5.3 dependency map).
8. Decide vector id strategy: either update the Vectra guide to reflect
   engine-assigned ids, or switch Lens to stable
   `VectorId = $"{source}:{chunkIndex}".GetHashCode64()` — the latter is
   required before Phase 2 per-file state can land.
