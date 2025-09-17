# Repository Guidelines

## Project Structure & Module Organization
- Analyzer code lives in `Gendarme.Analyzers`, grouped by rule category; when adding a rule, touch `Category.cs`, `DiagnosticId.cs`, and `Strings.resx` together.
- Shared Roslyn helpers sit under `Extensions/` (and `TypeSymbolExtensions.cs` in the root). Prefer extending these instead of creating new utilities.
- Tests mirror the analyzer folders inside `Gendarme.Analyzers.Tests` and reference the production project so diagnostics stay in sync.
- Build outputs (`bin/`, `obj/`) are git-ignored and should remain uncommitted.

## Build, Test, and Development Commands
- `dotnet restore` — restore dependencies for both projects.
- `dotnet build Gendarme.Analyzers/Gendarme.Analyzers.csproj` — compile analyzers with Roslynator checks enabled.
- `dotnet test Gendarme.Analyzers.Tests/Gendarme.Analyzers.Tests.csproj --no-build` — run the xUnit + Microsoft.CodeAnalysis harness.
- `dotnet format Gendarme.Analyzers/Gendarme.Analyzers.csproj` — optional cleanup to satisfy style diagnostics before review.

## Coding Style & Naming Conventions
- Code targets C# 12 with nullable and implicit usings; use four-space indentation and keep files single-responsibility.
- Name analyzer classes `<RuleName>Analyzer`; declare diagnostic IDs as PascalCase constants mapping to strings like `GENBP01`.
- The diagnostic ids should be in the class `DiagnosticId` and unique across the project.
- Store titles and messages in `Strings.resx` and access them through the generated `Strings` wrapper.
- Follow Roslynator suggestions (ordered usings, trailing commas, expression-bodied members where clearer) to keep builds clean.

## Testing Guidelines
- Tests use `CSharpAnalyzerTest<,>`; embed scenarios as raw string literals and assert `DiagnosticResult` against the expected ID.
- Mirror production categories: place positive and negative cases next to their analyzer peers, naming methods `Detects...` / `Skips...`.
- Apply the existing `TestOf` attribute to link tests and analyzers; add shared helpers only when multiple rules need them.
- For coverage spot-checks, run `dotnet test Gendarme.Analyzers.Tests/... --collect:"XPlat Code Coverage"` locally.

## Commit & Pull Request Guidelines
- Keep commits short, imperative, and lowercase (`fix design analyzer null checks`).
- Group work by analyzer or helper; avoid drive-by formatting without justification.
- PRs should mention affected diagnostic IDs, list new tests, and paste the latest `dotnet test` output; code snippets beat screenshots.

## Analyzer Authoring Tips
- Clone a nearby analyzer, then update its descriptor, localized strings, and category registration in one pass to prevent drift.
- Reuse extension helpers for symbol or syntax lookups before adding new ones.

## Localizable Strings
- Use the following pattern for localizable strings:
```cs
private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AttributeArgumentsShouldHaveAccessorsTitle), Strings.ResourceManager, typeof(Strings));
private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AttributeArgumentsShouldHaveAccessorsMessage), Strings.ResourceManager, typeof(Strings));
private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AttributeArgumentsShouldHaveAccessorsDescription), Strings.ResourceManager, typeof(Strings));

private static readonly DiagnosticDescriptor Rule = new(
    DiagnosticId.AttributeArgumentsShouldHaveAccessors,
    Title,
    MessageFormat,
    Category.Design,
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: Description
);
```
- Make sure that the names in `nameof(...)` match the keys in `Strings.resx`.
- Use `Strings.ResourceManager` and `typeof(Strings)` to ensure proper localization support.
- Keep the `DiagnosticDescriptor` initialization clean and consistent across different analyzers.
