# ADR-0001: Modular solution layout with central build policy

Status: Accepted (2026-11-05)

## Context
Lens is rebuilt from scratch. Module boundaries and build policy must be fixed
before integration adapters (Vectra, InstantAiGate) appear, otherwise each
adapter brings its own conventions.

## Decision
- src/ holds production projects named Instancium.Lens.*; tests/ mirrors them 1:1.
- builder/ holds executable pipeline scripts; artifacts/ is ignored output.
- Directory.Build.props owns all policy (TFM, nullable, warnings-as-errors, version).
- Directory.Packages.props owns all package versions (CPM with transitive pinning).
- Solution uses .slnx; test projects are identified by the .Tests name suffix.

## Consequences
- New modules add two folders and two slnx entries; no per-project policy drift.
- TFM upgrade is a one-line change in Directory.Build.props.
- Convention-based test detection forbids renaming test projects arbitrarily.
