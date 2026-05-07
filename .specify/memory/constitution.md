<!--
Sync Impact Report
==================
Version change: (new) → 1.0.0
Modified principles: N/A (initial population)
Added sections:
  - Core Principles (I–IV)
  - Performance Requirements
  - Quality Gates and Workflow
  - Governance
Removed sections: N/A (template placeholders cleared)
Templates updated:
  - .specify/templates/plan-template.md  ✅ Constitution Check gates aligned
  - .specify/templates/spec-template.md  ✅ Requirements section references principles
  - .specify/templates/tasks-template.md ✅ Task categories reflect principle-driven types
Deferred TODOs: none
-->

# Spec Kit .NET CLI Demo Constitution

## Core Principles

### I. Code Quality (NON-NEGOTIABLE)

Every line of production code MUST be clean, readable, and maintainable. Specifically:

- Code MUST follow the established style guide and linting rules for the target language (e.g.,
  .editorconfig, StyleCop, or Roslyn analyzers for .NET).
- Methods and functions MUST have a single, clearly defined responsibility.
- Cyclomatic complexity MUST be kept low; complex logic MUST be decomposed into
  well-named helpers.
- Dead code, commented-out blocks, and TODO comments MUST NOT be merged to the main
  branch without a linked issue.
- All public APIs MUST have XML documentation comments (for .NET) or equivalent
  inline documentation for the target language.

**Rationale**: Readable, well-structured code reduces defect rates, lowers onboarding cost,
and makes automated tooling (analyzers, formatters) effective over the long term.

### II. Test-First Development (NON-NEGOTIABLE)

Tests MUST be written before or alongside implementation. The Red-Green-Refactor cycle
is strictly enforced:

- Unit tests MUST be written before the feature implementation is considered complete.
- Every public method and CLI command MUST have at least one passing unit test.
- Integration tests MUST cover inter-component contracts and end-to-end CLI invocations.
- Tests MUST be deterministic; flaky or timing-dependent tests MUST NOT be merged.
- Test coverage MUST NOT fall below 80% on any new code introduced in a feature branch.
- Acceptance scenarios from the feature specification MUST map directly to automated tests.

**Rationale**: Test-first development surfaces design issues early, produces a living
specification of behavior, and prevents regressions as the codebase grows.

### III. User Experience Consistency

Every user-facing interaction (CLI output, error messages, help text) MUST follow a
consistent design language:

- CLI commands MUST follow the verb-noun convention (e.g., `spec create`, `spec run`).
- Success output goes to stdout; diagnostic messages, warnings, and errors go to stderr.
- All commands MUST support `--help` and produce structured output when `--format json`
  is supplied.
- Error messages MUST be actionable: they MUST state what went wrong and how to resolve it.
- Exit codes MUST be consistent: 0 for success, non-zero for failure, with documented
  codes for known error categories.
- Behavior MUST be predictable across platforms (Windows, macOS, Linux).

**Rationale**: A consistent, predictable interface reduces user error, improves learnability,
and makes the tool trustworthy as part of automated pipelines.

### IV. Performance Requirements

Performance goals are first-class requirements, not after-thoughts:

- CLI startup time MUST be below 200 ms on standard developer hardware.
- Any command that processes files or runs analysis MUST complete within 2 s for
  typical project sizes (up to 500 source files).
- Memory usage MUST NOT exceed 200 MB for any single command invocation.
- Performance budgets MUST be documented in the plan.md for each feature.
- Regressions against established baselines MUST block merge until resolved or the
  baseline is explicitly renegotiated with justification.

**Rationale**: A slow CLI creates friction in tight feedback loops. Performance commitments
set explicit expectations and keep the tool competitive for daily use.

## Performance Requirements

Detailed benchmarks and acceptance thresholds are recorded per feature in `plan.md`
under the "Performance Goals" field. The following apply globally:

- Benchmarks MUST be run in CI on every pull request targeting `main`.
- Baselines are stored in `.specify/memory/performance-baselines.json` (created per
  feature when first measured).
- Any result more than 10% above the baseline is a performance regression and MUST
  be addressed before merge.

## Quality Gates and Workflow

The following gates MUST pass before a feature branch is eligible for merge:

1. **Linting**: Zero linting errors (warnings are allowed but MUST be triaged).
2. **Tests**: All tests pass; coverage MUST be >= 80% on new code.
3. **Constitution Check**: Reviewer confirms all four principles are satisfied
   (referenced in `plan.md` Constitution Check section).
4. **Performance**: Benchmark results within 10% of baseline.
5. **UX Review**: At least one team member has exercised the CLI changes manually
   and confirmed output and error messages meet Principle III.

Pull requests MUST reference the associated spec and plan documents. Reviewers MUST
explicitly confirm the Constitution Check gate in their approval.

## Governance

This constitution supersedes all ad-hoc conventions, style guides, or informal
agreements within the repository. The following rules govern its amendment:

- **Amendment process**: Any team member may propose an amendment by opening a pull
  request that modifies `.specify/memory/constitution.md` and increments the version
  per the semantic versioning rules below.
- **Version semantics**:
  - MAJOR: Removal or redefinition of an existing principle (backward-incompatible
    governance change).
  - MINOR: Addition of a new principle or materially expanded guidance.
  - PATCH: Clarifications, wording improvements, or non-semantic refinements.
- **Ratification**: Amendments require approval from at least two maintainers.
- **Compliance review**: Constitution compliance is reviewed at the start of every
  planning cycle and after any significant architectural decision.
- **Technical decisions**: When evaluating architectural options, implementation
  approaches, or library choices, the option that best satisfies all four principles
  MUST be preferred, even if it requires more initial effort.
- **Runtime guidance**: See `.github/copilot-instructions.md` for AI-assisted
  development guidance that references these principles.

**Version**: 1.0.0 | **Ratified**: 2026-05-07 | **Last Amended**: 2026-05-07
