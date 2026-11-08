# Third-Party Notices & Licenses — Instancium Lens

This project uses third-party software components. Below is a comprehensive list
of these components, their licenses, and where to find their original license terms.

## 1. Direct Third-Party Dependencies (Runtime)

| Component | License | Usage |
| --- | --- | --- |
| [Microsoft.AspNetCore.SignalR.Client](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client) 10.0.14 | MIT | SignalR hub client; transport plane to the InstantAIGate gateway |
| [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions) 10.0.14 | MIT | Logging seam across all Lens modules |
| [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json) 10.0.14 | MIT | Bundled appsettings.json configuration source |
| [Microsoft.Extensions.Configuration.EnvironmentVariables](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.EnvironmentVariables) 10.0.14 | MIT | Environment overrides for secrets (e.g. Gateway__ApiKey) |
| [Microsoft.Extensions.Logging.Debug](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Debug) 10.0.14 | MIT | Debug logging provider in DEBUG builds |
| [Microsoft.Maui.Controls](https://www.nuget.org/packages/Microsoft.Maui.Controls), [Microsoft.Maui.Essentials](https://www.nuget.org/packages/Microsoft.Maui.Essentials), [Microsoft.AspNetCore.Components.WebView.Maui](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.WebView.Maui) ($(MauiVersion), pinned by the MAUI workload) | MIT | Native window shell, AppData path resolution (Microsoft.Maui.Storage), Blazor Hybrid WebView host |

## 2. Testing Libraries

| Component | License | Usage |
| --- | --- | --- |
| [xUnit](https://xunit.net/) (xunit.v3) 4.0.2 | Apache 2.0 | Primary unit testing framework |
| [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk) 18.11.0 | MIT | Test host and discovery |
| [coverlet](https://github.com/coverlet-oss/coverlet) (coverlet.collector) 10.1.0 | MIT | Cross-platform code coverage collector |

## 3. First-Party Instancium Components (Not Third-Party)

Lens consumes sibling Instancium products as NuGet packages or project references.
They are independent products with their own licensing and notices (baseline §2-§3)
and are listed here only for dependency-graph transparency:

| Component | Version | Role in Lens |
| --- | --- | --- |
| Instancium.Vectra | 1.1.1 | Local vector index engine (collections, cosine search, payload storage) |
| InstantAIGate.Core | 1.5.3 | Gateway DTO contracts (inference, session, status) |
| Instancium.Chunker, Instancium.FileParser, Instancium.Lens.* | 1.2.1 | Lens pipeline modules; source tree of this repository |

License terms for Instancium products ship with their packages and repositories.
The Lens solution itself is licensed under the LICENSE.txt at the repository root.

## 4. Transitive Inference Stack (Not Redistributed by Lens)

Lens contains no native inference code and no model weights. All model execution
is delegated over SignalR to InstantAIGate, which redistributes the following
components and carries their notices in its own THIRD-PARTY-NOTICES.md:

| Component | License | Source / Notes |
| --- | --- | --- |
| [llama.cpp](https://github.com/ggerganov/llama.cpp) | MIT | Native inference engine behind InstantAIGate |
| [ggml](https://github.com/ggerganov/ggml) | MIT | Tensor library and CPU/Vulkan backends |
| [Qwen](https://huggingface.co/Qwen) weights (qwen3-embedding-0.6b and chat models) | Apache 2.0 | Served by InstantAIGate from Hugging Face / ModelScope |

On Windows the Blazor Hybrid host additionally runs on the Microsoft Edge WebView2
runtime, an OS-provided Microsoft component that Lens does not redistribute.

## 5. UI Assets, Fonts and Icons

- No font files are redistributed with Lens. The CSS font stack prefers Inter and
  IBM Plex Sans (both SIL Open Font License 1.1 upstream) only when already
  installed on the user's machine and falls back to system families otherwise;
  a name-only reference creates no redistribution obligation.
- The application icon, splash screen, logo artwork and all CSS/JS assets under
  src/Instancium.Lens.App/wwwroot are first-party Instancium content.

## License Texts and Acknowledgements

### .NET Foundation & Microsoft Libraries
The .NET runtime, ASP.NET Core (including SignalR), .NET MAUI and the
Microsoft.Extensions.* libraries are licensed under the MIT License by the
.NET Foundation.

### xUnit
Copyright (C) .NET Foundation and contributors. xUnit is licensed under the
Apache License 2.0.

### coverlet
Copyright (c) coverlet contributors. Licensed under the MIT License.

### Qwen Model Family, llama.cpp and ggml (via InstantAIGate)
Copyright (c) Alibaba Cloud. The Qwen model weights are released under the
Apache License 2.0. Copyright (c) 2023-2026 Georgi Gerganov and contributors;
llama.cpp and ggml are licensed under the MIT License. Lens references these
components only through the InstantAIGate gateway process.

---
Maintenance rule: update this file in the same change that adds, removes or
upgrades a PackageReference in any *.csproj of the solution.
