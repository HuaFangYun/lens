# Changelog

All notable changes to the Instancium Lens project are documented in this file.
The format is based on Keep a Changelog; the project adheres to Semantic Versioning.

## [0.1.0] - 2026-11-05

### Added
- Solution skeleton InstanciumLens.slnx with modular layout (src/, tests/, builder/, docs/adr/).
- Central build policy Directory.Build.props (net10.0, nullable, warnings-as-errors, version baseline 0.1.0).
- Central package management Directory.Packages.props (CPM with transitive pinning).
- Instancium.Lens.Core: value-style failure primitives LensError, Result, Result<T>.
- xUnit v3 test harness Instancium.Lens.Core.Tests (11 tests).
- CI workflow: build + test on push and pull request.
- Single build entry point builder/build.ps1 (restore, build, test, optional pack).
- Repository tooling: builder/merge-codebase.ps1 and builder/split-codebase.ps1 for codebase snapshot exchange.
- ADR-0001: modular solution layout with central build policy.
