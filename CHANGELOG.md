# Changelog

All notable changes to the Instancium Lens project are documented in this file.
The format is based on Keep a Changelog; the project adheres to Semantic Versioning.

## [0.1.0] - 2026-11-05

### Added
- Solution skeleton InstanciumLens.slnx with modular layout (src/, tests/, builder/).
- Central build policy (Directory.Build.props) and central package management (Directory.Packages.props).
- Instancium.Lens.Core: value-style failure primitives LensError, Result, Result<T>.
- xUnit v3 test harness Instancium.Lens.Core.Tests (11 tests).
- builder/build.ps1 single entry point for build pipeline.
