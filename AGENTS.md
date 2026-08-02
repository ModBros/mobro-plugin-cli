# Project Overview

`mobro-plugin-cli` is the official .NET global tool for the MoBro platform. It gives plugin developers a single `mobro`
command to build, package, locally install (for testing against a running MoBro instance), and publish plugins built
with the [MoBro Plugin SDK](https://github.com/ModBros/mobro-plugin-sdk) to the MoBro marketplace.

## Repository Structure

- `MoBro.Plugin.CLI/` – main CLI project (packed as the `mobro` dotnet tool).
    - `CliArgs/` – `CommandLineParser` verb/argument definitions (one class per CLI command).
    - `CliActions/` – the corresponding command implementations invoked for each verb.
    - `Helper/` – cross-cutting utilities: console output, plugin metadata reading, plugin packaging/publishing, API
      client factory.
    - `Marketplace/` – Refit interfaces and request/response models for the MoBro marketplace HTTP API.
    - `MoBro/` – Refit interface and response models for the local MoBro service API (`localhost:42069`).
    - `Model/` – shared plain data models (`BuildInfo`, `PluginMeta`).
    - `Resources/` – package icon/app icon assets.
    - `Program.cs` – CLI entry point wiring verbs to actions.
    - `Constants.cs` – base URLs, well-known file names.
- `MoBro.Plugin.CLI.Tests/` – xUnit + Moq test project, one test class per CLI action, mirroring `CliActions/`.
- `CHANGELOG.md` – Keep a Changelog-formatted release history.
- `publish_baget.bat` – Windows batch script to build and push the NuGet package to the internal BaGet server.
- `mobro-plugin-cli.sln` – Visual Studio / Rider solution file referencing both projects.

## Build & Development Commands

Build the solution:

```
dotnet build
```

Run all tests:

```
dotnet test
```

Run a single test project/class:

```
dotnet test MoBro.Plugin.CLI.Tests --filter FullyQualifiedName~PublishActionTests
```

Install the published tool globally (end-user usage, from NuGet):

```
dotnet tool install --global MoBro.Plugin.Cli
```

## Code Style & Conventions

- Language: C# 14, target framework `net10.0`, `Nullable` and `ImplicitUsings` enabled.
- Indentation: 2 spaces (consistent across the codebase); braces on their own line.
- Classes are `internal sealed class` by default; only interfaces and DTOs consumed across project boundaries are
  `public`.
- One CLI verb = one `*Args` class (in `CliArgs/`) + one matching `*Action` class (in `CliActions/`), named identically
  apart from the suffix.
- Interfaces are prefixed with `I` (e.g. `IApiClientFactory`, `ICliConsole`) and implementations are
  constructor-injected into actions.
- File-scoped namespaces (`namespace MoBro.Plugin.Cli.X;`).
- No `.editorconfig` or analyzer ruleset is present; follow existing file formatting when editing.
- Commit messages: no enforced template found in the repo.

## Architecture Notes

```
 ┌────────────┐      parses argv      ┌─────────────────┐
 │ Program.cs │ ────────────────────▶ │ CliArgs (Verbs) │
 └────────────┘                       └───────┬─────────┘
                                              │ dispatch
                                              ▼
                                      ┌───────────────┐
                                      │  CliActions   │
                                      └───────┬───────┘
                     ┌────────────────────────┼───────────────────────┐
                     ▼                        ▼                       ▼
             ┌───────────────┐        ┌───────────────┐       ┌───────────────┐
             │ Helper/       │        │ MoBro/ (Refit)│       │ Marketplace/  │
             │ (metadata,    │        │ local MoBro   │       │ (Refit)       │
             │ packaging,    │        │ service API   │       │ marketplace   │
             │ console I/O)  │        │ :42069        │       │ HTTP API      │
             └───────────────┘        └───────────────┘       └───────────────┘
```

- `Program.cs` wires `CommandLineParser` verbs (`PublishArgs`, `InstallArgs`, `MarketplacePublishArgs`, etc.) to their
  `CliActions` counterparts.
- `PublishAction` reads plugin metadata (`PluginMetaDataReader`) from a project directory's `mobro_plugin_config.json`,
  builds the project, and zips it (`PluginPublisher`) to an output folder.
- `InstallAction` reuses publish logic (if given a project dir) then calls the local MoBro service API (
  `IMoBroServicePluginApi`, `http://localhost:42069/api`) to install the plugin into a running MoBro instance.
- `MarketplacePublishAction`/`MarketplaceUpdate*Action` classes call the marketplace HTTP API (`IMarketplacePluginApi`,
  `IMarketplacePluginVersionApi`, `IMarketplaceResourceApi`) via Refit-generated clients, created through
  `ApiClientFactory`, to create/update plugin listings, logos, store pages, and install notices. A `--dev` flag switches
  between production and dev marketplace base URLs.
- Data flow: CLI args → action → helper(s) for local file/plugin operations and/or Refit API client for HTTP calls →
  console output via `ICliConsole`.

## Testing Strategy

- Unit tests only, using **xUnit** as the test framework and **Moq** for mocking dependencies (`ICliConsole`,
  `IApiClientFactory`, `IPluginMetaDataReader`, `IPluginPublisher`, etc.).
- Test project: `MoBro.Plugin.CLI.Tests`, mirroring `CliActions/` 1:1 (e.g. `PublishActionTests.cs` tests
  `PublishAction.cs`).
- Run locally:

```
dotnet test
```

## Security & Compliance

- The marketplace API requires an API key (`--api-key` option on `marketplace-publish` and marketplace update commands);
  never hard-code real API keys in source or tests.
- No secrets are stored in the repository; `.gitignore` excludes `bin/`, `obj/`, `_ReSharper.Caches/`, and IDE/build
  artifacts.
- License: MIT (see `LICENSE`), Copyright ModBros KG.
- Dependencies are managed via NuGet (`CommandLineParser`, `Refit`) and test-only packages (`xunit`, `Moq`,
  `coverlet.collector`).

## Agent Guardrails

- Do not commit real MoBro marketplace API keys, tokens, or credentials in code, tests, or `Program.cs` sample args.
- Do not modify `LICENSE` or the `PackageLicenseExpression` in `MoBro.Plugin.CLI.csproj` without explicit maintainer
  approval.
- `CHANGELOG.md` should be updated for any user-facing change, following the existing Keep a Changelog format.
- Changes to `Constants.cs` base URLs (marketplace/MoBro service) affect all users of the published tool — require
  review before altering.
- Version bumps (`VersionPrefix` in `MoBro.Plugin.CLI.csproj`) and publishing (`publish_baget.bat`, NuGet push) should
  only be performed by a maintainer/release process, not automatically by an agent.

## Extensibility Hooks

- New CLI commands are added by creating a new `*Args` class in `CliArgs/` (decorated with `[Verb(...)]`) and a matching
  `*Action` class in `CliActions/`, then registering both in the `Parser.Default.ParseArguments<...>()` call and
  `.WithParsed<...>()` chain in `Program.cs`.
- `IApiClientFactory` centralizes creation of Refit API clients — new marketplace or MoBro service endpoints should be
  added as interfaces under `Marketplace/` or `MoBro/` and exposed through this factory.
- Environment/config values:
    - `Constants.MarketPlaceBaseUrl` / `MarketPlaceBaseUrlDev` – marketplace API base URLs, selected via the `--dev`
      flag on marketplace commands.
    - `Constants.MoBroServiceBaseUrl` – local MoBro service endpoint (`http://localhost:42069/api`).
    - `Constants.PluginConfigFile` (`mobro_plugin_config.json`) and `Constants.BuildInfoFile` (`build_info.json`) –
      well-known plugin metadata file names read/written during publish.
    - `Constants.DefaultPluginAssembly` (`Plugin.dll`) – expected plugin entry assembly name.
