# Phase 0: Research

## Location and Timezone Lookups

**Decision**: Use `TimeZoneConverter` (to handle IANA/Windows timezone mappings) and `NodaTime` for safe Date/Time timezone calculations. For Geocoding (ZIP/City to lat/long/timezone), we will use an external API such as the *Open-Meteo Geocoding API* with a gracefully degrading fallback, failing with an actionable message if offline. 
**Rationale**: Native `TimeSpan` logic is insufficient for advanced multi-region scheduling safely across DST boundaries. Building a fully offline US ZIP/Global city database would bloat the single-binary deployment unnecessarily. 
**Alternatives considered**: Embedded SQLite DB of global locations (rejected due to size and maintenance overhead).

## Single Binary & Cross-Platform Support (.NET)

**Decision**: Target .NET 8 LTS using Native AOT (`<PublishAot>true</PublishAot>`) for the final builds or self-contained single file publish (`<PublishSingleFile>true</PublishSingleFile>`) if AOT has compatibility issues with specific reflection-heavy libraries. Use GitHub Actions for multi-tier OS/Arch matrix builds.
**Rationale**: Single file deployments natively provided by .NET satisfy the requirement and Native AOT guarantees the `<200ms` startup performance constraint outlined in the constitution.
**Alternatives considered**: Traditional framework-dependent builds (rejected, violates single binary request).

## CLI Parsing & UX

**Decision**: Use `Spectre.Console` combined with `Spectre.Console.Cli`.
**Rationale**: Automatically handles verb-noun parsing, cross-platform terminal UX, and structured text tables which matches the spec explicitly. Easy to drop into `--format json` using custom formatters.
**Alternatives considered**: `System.CommandLine` (rejected because Spectre provides better built-in table layout support for the "Propose Meeting" feature).