# Quickstart: Timezone Meeting CLI

## Setup Environment
1. Ensure `.NET 8.0 SDK` is installed.
2. Clone the repository.

## Running Locally
During development, use `dotnet run` to invoke the CLI:

```bash
# Lookup time by Zip Code
dotnet run --project src/TimezoneMeetingCli -- time lookup 10001

# Lookup time by City (wrap spaces in quotes)
dotnet run --project src/TimezoneMeetingCli -- time lookup "London"

# Propose a meeting across multiple timezones
dotnet run --project src/TimezoneMeetingCli -- time schedule "14:00" "NYC" "London" "Tokyo" "Berlin" "Sydney"
```

## Running Tests
Tests enforce the Constitution's >80% coverage and Red-Green cycle:
```bash
dotnet test
```

## Creating the Single File Deployments
Create the OS-specific single-file binaries natively using `dotnet publish`:

```bash
# Windows (x64)
dotnet publish src/TimezoneMeetingCli -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true

# macOS (ARM64 / Apple Silicon)
dotnet publish src/TimezoneMeetingCli -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained true

# Linux (x64)
dotnet publish src/TimezoneMeetingCli -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true
```
The output execution binaries will be found in their respective `bin/Release/net8.0/[RID]/publish/` folders.