---
name: spec-update
description: Change docs/spec.md consistently — apply a feature request or design decision across all affected sections, bump the version, add a changelog row and an ADR.
argument-hint: "[what to change]"
disable-model-invocation: true
---

# Update the TaskFlow spec

Requested change: `$ARGUMENTS`

The spec is written in Russian; keep its style and terminology.

## Steps

1. Read `docs/spec.md` fully (it is long — read it in parts). Find every place the change touches. Typical cross-references:
   - §1 features (MVP vs backlog), §1.3 guarantees
   - §2 components and key decisions
   - §3 state machine, sequence diagram, failure table
   - §4 RabbitMQ queues/stream and messages
   - §5 EF Core model, saga attempts, Idempotency-Key, Dispatcher, cancel
   - §6 worker algorithm and guarantees
   - §7 API, §8 UI, §9 metrics
   - §11 ADR, §12 milestones and DoD, §13 timeouts, §15 DAG, §16 open questions
2. If the change is a design decision with alternatives, discuss it with the user **before** editing: options, trade-offs, recommendation.
3. Apply the change to every affected section so the document stays consistent (statuses, field names, timeouts, DoD).
4. Version: minor bump (`v1.0 → v1.1`) for new features or decisions, patch bump (`v1.1 → v1.1.1`) for clarifications. Update the header.
5. Add a row to «История изменений» at the end: version, date, what changed with section numbers.
6. For a decision, add an ADR to §11: problem, options considered, decision, cost.
7. Show the user a short summary of what changed, by section. Do not commit — the user commits.
