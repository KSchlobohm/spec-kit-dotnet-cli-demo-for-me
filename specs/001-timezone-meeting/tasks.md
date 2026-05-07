---
description: "Task list template for feature implementation"
---

# Tasks: Timezone Meeting CLI

**Input**: Design documents from `/specs/001-timezone-meeting/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

**Constitution gates for this phase**:
- Principle I (Code Quality): configure linting and analyzer rules before any source is written.
- Principle II (Test-First): set up test project/runner so tests can be written from T004 onward.
- Principle III (UX Consistency): establish CLI command structure and output conventions.
- Principle IV (Performance): define performance baseline measurement task.

- [ ] T001 Create project structure per implementation plan (`src/TimezoneMeetingCli`, `tests/TimezoneMeetingCli.UnitTests`, `tests/TimezoneMeetingCli.IntegrationTests`)
- [ ] T002 Initialize .NET 8 LTS project with Spectre.Console, NodaTime, and TimeZoneConverter dependencies
- [ ] T003 [P] Configure linting, formatting, and static analysis tools (Principle I)
- [ ] T004 [P] Set up test project and runner; confirm red-green cycle works (Principle II)
- [ ] T005 [P] Document CLI command naming conventions and output format contract in a shared class (Principle III)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T006 Set up base DI container (`Spectre.Console.Cli` TypeRegistrar) in `src/TimezoneMeetingCli/Program.cs`
- [ ] T007 Implement the core `LocationQuery`, `GeocodedLocation`, and `LocalizedTimeResult` entities in `src/TimezoneMeetingCli/Models/`
- [ ] T008 [P] Implement `OpenMeteoGeocodingService` (or similar) in `src/TimezoneMeetingCli/Services/Geocoding/` with standard HTTP fallback error handling
- [ ] T009 [P] Implement `NodaTimeConversionService` in `src/TimezoneMeetingCli/Services/TimeConversion/` passing basic IANA tests
- [ ] T010 Setup basic Application output handler to conditionally format plain text vs json (based on `--format`)

---

## Phase 3: User Story 1 - Locate Current Date and Time (Priority: P1) 🎯 MVP

**Goal**: As a user, I want to look up the current date and time for a specific location using a timezone name, city/region name, or US zip code, so that I can easily know what time it is there without opening a browser.

**Independent Test**: Execute the lookup command with a known ZIP code, city name, and timezone identifier. Verify that standard stdout prints the accurate, localized date and time in a human-readable format.

- [ ] T011 [P] [US1] Create unit tests for parsing standard and edge-case zip codes and city names in `tests/TimezoneMeetingCli.UnitTests/`
- [ ] T012 [US1] Implement the `time lookup` command class (`LookupCommand`) in `src/TimezoneMeetingCli/Commands/`
- [ ] T013 [US1] Wire the `LookupCommand` arguments to the Geocoding and TimeConversion services
- [ ] T014 [US1] Implement structured grid output vs JSON output handlers for single location results
- [ ] T015 [US1] Create integration test verifying single command invocation returns correct data in `tests/TimezoneMeetingCli.IntegrationTests/`

---

## Phase 4: User Story 2 - Propose Meeting Times (Priority: P2)

**Goal**: As a user, I want to input my local time or a proposed meeting time along with a list of participant locations (timezones, cities, or zip codes) so that the CLI can calculate and display what time the meeting will occur for each participant.

**Independent Test**: Provide a base time in one configuration and multiple target locations as arguments. Verify that standard output lists the equivalent times for all target locations clearly.

- [ ] T016 [P] [US2] Create unit tests testing cross-timezone array resolutions for proposed base times in `tests/TimezoneMeetingCli.UnitTests/`
- [ ] T017 [US2] Implement `MeetingProposal` entity in `src/TimezoneMeetingCli/Models/`
- [ ] T018 [US2] Implement the `time schedule` command class (`ScheduleCommand`) in `src/TimezoneMeetingCli/Commands/`
- [ ] T019 [US2] Implement the table renderer for multiple locations leveraging `Spectre.Console`
- [ ] T020 [US2] Create integration test verifying meeting scheduling command with 5+ locations succeeds and fails gracefully on bad input

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T021 Build and confirm self-contained single binaries for target architectures (win-x64, osx-arm64, linux-x64).
- [ ] T022 Document and verify performance limits to meet <200ms CLI startup and <200MB constraints.
