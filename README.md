# Instancium Lens

**Local AI Workspace built on top of Instancium.Vectra**

Instancium Lens is a desktop application for exploring knowledge contained in your documents using natural language.

Built as a lightweight presentation layer over the **Instancium.Vectra.Sdk**, Lens provides a user-friendly interface for document indexing, semantic search, and retrieval-augmented generation (RAG) without requiring external cloud services.

All processing can be performed locally through InstantAIGate and Vectra, allowing users to work with confidential information while maintaining full control over their data.

---

## Why Lens?

Traditional document search helps you find text.

Lens helps you:

* Understand large document collections.


* Ask questions in natural language.


* Retrieve relevant source material.


* Validate information against original sources.


* Keep sensitive data local.



Instead of manually navigating hundreds of pages, Lens helps surface relevant context and explain why it matters.

---

## Built on Vectra SDK

Lens uses **Instancium.Vectra.Sdk** as its embedded retrieval and knowledge foundation.

### Vectra SDK provides

* PDF document ingestion.


* Text chunking.


* Embedding generation.


* Local disk-backed vector storage.
* High-performance semantic search.



### Lens focuses on the user experience layer

* Desktop UI.


* Workspace management.


* Source navigation.


* AI-assisted investigation.



---

## Architecture: Thin Client Model

```text
┌──────────────────────────────────┐
│       Instancium Lens App        │
│  (MAUI Blazor Thin Client / UI)  │
└────────────────┬─────────────────┘
                 │
     IndexFolder / SearchAsync
                 ▼
┌──────────────────────────────────┐
│      Instancium.Vectra.Sdk       │
│  (Local Search & Index Engine)   │
└────────────────┬─────────────────┘
                 │
      Embeddings & LLM Streaming
                 ▼
┌──────────────────────────────────┐
│        InstantAIGate.Core        │
│        (Local AI Gateway)        │
└──────────────────────────────────┘

```

---

## Core Features

### Document Intelligence

Import documents and transform them into searchable knowledge.

* PDF processing.


* Semantic indexing.


* Natural language retrieval.



### AI-Assisted Knowledge Exploration

Ask questions about your document collection.

**Examples:**

* "Which sections discuss pressure sensors?"


* "What documents mention this requirement?"



### Source Traceability

Answers should always be connected to source material.

Lens is designed around the principle that **AI is not the source of truth**. Claims should be traceable back to the original documents whenever possible.

### Local-First Architecture

* Local document storage.


* Local indexing.


* Local AI inference through InstantAIGate.


* No mandatory cloud dependency.



---

## Vision

The long-term goal of Lens is **not** to become another chat application.

The goal is to create a workspace where users can:

* Understand complex information domains.


* Trace conclusions back to evidence.


* Build confidence in decisions.



**Vectra remains the retrieval foundation. Lens becomes the experience layer built on top of it.**

---

## Technology Stack

* .NET 10


* C#


* Instancium.Vectra.Sdk
* InstantAIGate.Core
* SignalR


* MAUI Blazor Hybrid



---

## Status

**Current Stage:** MVP / R&D

The primary focus is validating document ingestion, retrieval quality, and the overall Lens user experience.