# CLI Interface Contract

## Global Options
- `--format` (string): Output format, defaults to `text`. Valid options: `text`, `json`.
- `-h, --help`: Prints usage help.

## Command 1: `time lookup`
**Description**: Locates the current date and time for a given location or timezone.

**Arguments**:
- `<location>` (string, required): The target location (e.g., "10001", "London", "PST").

**Examples**:
- `time lookup 10001`
- `time lookup "Paris, France"`
- `time lookup Europe/Paris --format json`

---

## Command 2: `time schedule`
**Description**: Display the equivalent times for a proposed host meeting time across multiple participant locations.

**Arguments**:
- `<time>` (string, required): The local time to schedule, e.g., "2026-05-15T14:00" or just "14:00" (assumes relative upcoming).
- `<locations...>` (string[], required): A space-separated list of target participant locations (min 5 as per constraints).

**Examples**:
- `time schedule "15:00" "London" "Tokyo" "10001" "America/Los_Angeles" "Sydney"`
- `time schedule "2026-05-08T09:00" "London" "Tokyo" "10001" "America/Los_Angeles" "Sydney" --format json`