## [1.2.0] - 2026-11-06

### Architectural decisions

- **ADR-001 (binding):** Real shipped API of `Instancium.Vectra` 1.1.1 and
  `InstantAIGate.Core` 1.5.5 is the single source of truth. The two integration
  guides (`Vectra Integration Guide`, `InstantAIGate Guide v1.5`) are
  descriptive documents that lag behind the shipped surface and must be
  updated to match the code — never the other way around. Lens MVP ships
  against the real API; library-level improvements (if any) land **after**
  MVP and never gate the Lens release pipeline.

### Intentional deviations from the technical baseline

- Product version **1.2.0**; `Instancium.Lens.Core` is `1.2.0-rc.2`,
  remaining Lens modules are `0.1.0` (alignment pending, non-blocking).
- `InstantAIGate.Core` pinned to **1.5.3** (server ships 1.5.5 — binary
  compatible; bump is a housekeeping item, not a blocker).
- `Instancium.Vectra` pinned to **1.1.1** (correct package id; the guide
  incorrectly refers to `Instancium.Vectra.Sdk` 1.1.0-rc.6 — guide to be
  corrected).

### Guide drift (guides to be updated)

These are NOT defects in the Lens codebase. The guides document a stale
surface; Lens code targets the real, shipped API and must remain untouched
until MVP ships.

1. **Vectra Integration Guide — stale against `Instancium.Vectra` 1.1.1.**
   Lens correctly consumes the real surface:
   `GetOrOpenCollection(name, CollectionOptions)`,
   `CollectionOptions(dimension, metric, modelId, modelFingerprint, quantization)`,
   `SimilarityMetric` enum,
   `VectorEntry(long? vectorId, ReadOnlyMemory<float> vector, string? payload)`
   (engine auto-assigns id when `vectorId` is null),
   instance extension `collection.SearchTextAsync(provider, query, options)`,
   `collection.GetPayloadTextAsync(long vectorId)`,
   `VectorSearchHit` with hydrated `Payload` field.
   Guide documents obsolete names: `GetOrCreateCollectionAsync`,
   `CollectionSpec`, `Metric`, `VectraRecord` (string id),
   static `VectraTextExtensions.SearchTextAsync`, `SearchHit`,
   `GetPayloadTextAsync(hit.Id)`, and wrong package id
   `Instancium.Vectra.Sdk`. → Guide must be rewritten to match §3 of
   `Instancium.Vectra.Core/Contracts`.

2. **Vector IDs are engine-assigned** (`VectorEntry(vectorId: null, ...)`).
   The Vectra guide prescribes stable per-`(source, chunkIndex)` ids.
   Tolerable for MVP delete-recreate strategy; per-file index state
   (Phase 2) will require a stable id strategy — this is a Phase 2 concern,
   **not** an MVP blocker.

3. **`GatewayChatClient` — event subscriptions after `StartAsync`.**
   Guide §10 checklist item "register all 7 events before `StartAsync`" is
   violated. Not a correctness bug for single-session use; concurrent
   sessions on one `HubConnection` would need per-`sessionId` filtering.
   Deferred to post-MVP hardening.

4. **No operation timeouts in `GatewayOptions`** (guide §8 prescribes
   100 s / 30 s). Deferred to post-MVP; current hot path uses caller-side
   cancellation.

5. **No session-not-found retry** (guide §7 prescribes one retry with
   session re-creation). Single-session MVP does not hit this path.

6. **Naming**: roadmap's `GatewayLlmClient` is implemented as
   `GatewayChatClient` — naming-only, no behavioural impact.

7. **Module version drift** (1.2.0-rc.2 vs 0.1.0). Non-blocking; align
   before 1.2.0 GA tag.

### Not implemented (Phase 1 exit criteria)

- `Instancium.Lens.App` composition root (.NET MAUI Blazor Hybrid, DI,
  Home/Chat UI, `config_vectra.json`, user secrets). **Confirmed absent.**
  This is the single remaining blocker for MVP exit.
- Architecture tests (`tests/Instancium.Lens.Architecture.Tests`) enforcing
  §5.3 dependency rules.
- End-to-end validation evidence with real embedding and LLM models.

### Changed

- **ADR-003:** MVP platform scope reduced to Windows only
  (`net10.0-windows10.0.19041.0`). Android, iOS and MacCatalyst TFMs and their
  platform bootstraps are removed from `Instancium.Lens.App`; the
  multi-platform matrix is deferred post-MVP and restorable from git history.
  Intentional deviation from baseline §9 ("MVP validated on Windows and
  MacCatalyst first"), approved by PO under Occam's razor: validation effort
  on unshipped platforms was blocking the MVP pipeline.s
