# Repository Guidelines

## Project Structure & Module Organization

The root solution, `ReSharper.Structured.Logging.slnx`, groups the .NET plugin, tests, build automation, and documentation. Core ReSharper and Rider backend code lives in `src/ReSharper.Structured.Logging/`, organized by responsibility (`Analyzer/`, `Highlighting/`, `QuickFixes/`, `Settings/`, and related support folders). Rider frontend Kotlin and resources are under `src/rider/main/`. NUnit fixtures are in `test/src/`; analyzer and quick-fix inputs plus expected `.gold` output files are in `test/data/`. Rule documentation belongs in `rules/`, screenshots in `images/`, and NUKE orchestration in `build/Build.cs`.

## Build, Test, and Development Commands

Run commands from the repository root:

- `build.cmd Compile` (Windows) or `./build.sh Compile` (Unix): restore and compile the ReSharper plugin.
- `build.cmd Test`: build the test project and run it with NUnit Console.
- `build.cmd Pack`: compile and create the ReSharper NuGet package; this is the default NUKE target.
- `build.cmd PackRiderPlugin --is-rider-host`: build and package the Rider plugin through Gradle.
- `dotnet sln ReSharper.Structured.Logging.slnx list`: verify that solution project links resolve.

The build requires a compatible .NET SDK; Rider packaging also requires JDK 17. Generated output appears in `bin/`, `gradle-build/`, and repository-root package files and must not be committed.

## Development Reference

When implementing features or editing ReSharper/Rider plugin code, consult the [ReSharper Platform SDK documentation](https://www.jetbrains.com/help/resharper/sdk/welcome.html) for supported APIs, extension points, and platform guidance.

## Coding Style & Naming Conventions

Follow `.editorconfig`: UTF-8, spaces, final newlines, and trimmed trailing whitespace. Use four-space indentation for C# and Kotlin; use two spaces for project, JSON, YAML, XML, props, and NuSpec files. Keep `System` imports first and separate import groups. Use PascalCase for types and members, `_camelCase` for private C# fields, and descriptive analyzer/highlighting names such as `PropertiesNamingAnalyzer` and `InconsistentLogPropertyNamingWarning`. Preserve the surrounding namespace style when editing older files.

## Testing Guidelines

Tests use NUnit with JetBrains ReSharper test infrastructure. Name fixtures `*Tests` and test methods `Test<Scenario>`. For analyzer or quick-fix behavior, add matching files under `test/data/<Category>/<Feature>/`; update both the `.cs` input and `.cs.gold` expectation. No numeric coverage threshold is configured, but behavior changes should include regression tests. Run `build.cmd Test` before submitting.

## Commit & Pull Request Guidelines

Recent history uses short, imperative subjects such as `Add ZLogger support` and `Fix naming analyzer cast issue`, often followed by an issue or PR number. Keep each commit focused. Pull requests should explain the behavior change, link relevant issues, list validation commands, and include screenshots for visible Rider UI changes. Never add generated-by text, signatures, email attribution, or `Co-Authored-By` footers to commits or pull requests.
