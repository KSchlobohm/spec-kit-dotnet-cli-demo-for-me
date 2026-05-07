# Phase 1: Data Model

## Core Entities

### LocationQuery
Represents the user's raw input for a location constraint.
- `RawInput` (string): The actual string the user provided (e.g., "10001", "London", "America/New_York").
- `QueryType` (Enum): `ZipCode`, `CityName`, `TimezoneId`.

### GeocodedLocation
The resolved geographical information for a `LocationQuery`.
- `Name` (string): Standardized name (e.g., "London, UK").
- `Latitude` (double): Latitude used for timezone extraction if needed.
- `Longitude` (double): Longitude.
- `TimezoneId` (string): canonical IANA timezone ID (e.g., "Europe/London").

### LocalizedTimeResult
The output DTO for standard lookup display.
- `Query` (string): Echoing the raw query.
- `ResolvedLocation` (string): Standardized location name.
- `LocalTime` (DateTimeOffset): The calculated local time.
- `UtcOffset` (TimeSpan): The offset relative to UTC for the specific query moment.
- `TimezoneAbbreviation` (string): e.g., "EST", "BST".

### MeetingProposal
- `BaseTime` (DateTimeOffset): The proposed host time.
- `ParticipantResults` (List<LocalizedTimeResult>): The times translated for every target location.