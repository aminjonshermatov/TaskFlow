---
name: brief
description: Prepare the user to implement a TaskFlow GitHub issue himself — plan, concepts to learn, pitfalls, how to verify — without writing the code.
argument-hint: "[issue-number]"
disable-model-invocation: true
allowed-tools: Bash(gh issue view *) Bash(git status *) Bash(git log *)
---

# Task briefing

Issue: `$ARGUMENTS`

1. Read the issue: `gh issue view $ARGUMENTS --comments`.
2. Read the spec sections it references in `docs/spec.md` and the existing code it touches.
3. Answer in Russian with:
   - **Что сделать** — the goal in your own words, 2–3 sentences.
   - **План** — 3–7 ordered steps, each small enough to commit separately. Steps describe *what*, not the code.
   - **Что изучить** — the C#/.NET concepts the task exercises, with one link each (learn.microsoft.com or the library's docs) and a one-line C++ comparison where helpful.
   - **Подводные камни** — what usually goes wrong here (races, cancellation, disposal, spec constraints).
   - **Как проверить** — how to verify the Definition of Done from the issue: commands, tests to write, what to look at.
4. Do not write implementation code. If the task needs an exercise first (an unfamiliar concept), suggest a 20–30 line console experiment the user can do before touching the project.
5. End by reminding: `/hint` when stuck, `/mentor-review` before commit.
