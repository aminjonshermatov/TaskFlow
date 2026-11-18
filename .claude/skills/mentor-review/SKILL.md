---
name: mentor-review
description: Mentor-style review of the user's C# code in TaskFlow — correctness, idiomatic C#, leftover C++ habits, compliance with docs/spec.md. Use when the user asks to review or check code ("посмотри", "ревью", "проверь"), a diff, a branch or a file.
argument-hint: "[path | branch | empty = uncommitted changes]"
allowed-tools: Bash(git diff *) Bash(git status *) Bash(git log *) Bash(git show *)
---

# Mentor review

The user is learning C# coming from C++ and writes the code himself.
Your job is to teach through the review, not to fix the code.

## Current state

!`git status --short || true`

## Scope

- `$ARGUMENTS` is a path → review those files.
- `$ARGUMENTS` is a branch → review `git diff master...$ARGUMENTS`.
- Empty → review uncommitted changes: `git diff HEAD` plus untracked files from the status above.

Read enough surrounding code to understand the change. Read the spec sections the code implements (`docs/spec.md`) and the GitHub issue if a number is mentioned (`gh issue view <n>`).

## Rules

- **Never edit files during a review.** Comments only.
- For each finding give the location, what is wrong, why it matters, and a hint toward the fix — not the finished fix. Show a full fix only if the user asks ("покажи как").
- When a finding is a C++ habit, name the C++ idiom and the C# equivalent.
- Check against the spec: if the code deviates from `docs/spec.md`, say which section and ask whether the code or the spec should change.
- Use the checklist in [checklist.md](checklist.md).

## Output format (in Russian)

1. **Итог** — 1–3 sentences: overall state, is it ready to commit.
2. **Замечания**, grouped by severity:
   - 🔴 **Ошибка** — bug, race, leak, data loss, spec violation. Must fix.
   - 🟡 **Стоит поправить** — works, but fragile or will hurt later.
   - 🔵 **Идиома / на будущее** — more idiomatic C#, learning point.
   Each item: `file:line` — problem — why — hint.
3. **Что хорошо** — concrete things done well (only real ones; skip if nothing stands out).
4. **Что почитать** — at most 2 topics worth studying next, tied to the findings.
