# Timezone Meeting CLI

A cross-platform, single-binary CLI tool for effortlessly looking up current times across the globe and proposing meeting times across multiple timezones.

Built with .NET 8 Native AOT/Single-File, Spectre.Console, and NodaTime.

## Features

- **Lookup Time:** Instantly get the current local time for a specific Timezone, City Name, or US Zip Code.
- **Schedule Meetings:** Propose a local time and automatically calculate the equivalent times for 5+ other global participants.
- **Pretty Output:** Visual grid and table layouts natively in your terminal.
- **JSON Support:** Built-in `--format json` flag for automation and script piping.
- **Cross-Platform:** Available as a single, self-contained binary for Windows, macOS, and Linux. No .NET runtime required to execute.

## Commands

### `time lookup`
Locates the current date and time for a given location or timezone.

**Usage:**
```bash
TimezoneMeetingCli time lookup <location> [OPTIONS]
```
**Examples:**
```bash
TimezoneMeetingCli time lookup 10001
TimezoneMeetingCli time lookup "Paris, France"
TimezoneMeetingCli time lookup Europe/Paris --format json
```

### `time schedule`
Display the equivalent times for a proposed host meeting time across multiple participant locations.

**Usage:**
```bash
TimezoneMeetingCli time schedule <time> <locations...> [OPTIONS]
```
**Examples:**
```bash
TimezoneMeetingCli time schedule "14:00" "London" "Tokyo" "10001" "America/Los_Angeles" "Sydney"
TimezoneMeetingCli time schedule "2026-05-15T09:00" "New York" "London" "Tokyo" --format json
```

## Development & Publishing

### Running Locally
During development, you can use the `dotnet run` command:
```bash
dotnet run --project src/TimezoneMeetingCli -- time lookup "London"
```

### Building Single Binaries
To build the standalone executables for your target platform:

```bash
# Windows (x64)
dotnet publish src/TimezoneMeetingCli -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true

# macOS (ARM64 / Apple Silicon)
dotnet publish src/TimezoneMeetingCli -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained true

# Linux (x64)
dotnet publish src/TimezoneMeetingCli -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true
```
The output execution binaries will be found in their respective `bin/Release/net8.0/[RID]/publish/` folders.

## Testing
Run the test suite (Unit & Integration tests) using:
```bash
dotnet test
```