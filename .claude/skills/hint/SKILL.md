---
name: hint
description: Graded hint for a TaskFlow task or bug the user is stuck on, without writing the solution.
argument-hint: "[1|2|3] [question or #issue]"
disable-model-invocation: true
---

# Hint

The user is learning C# and wants to solve the problem himself. Give **only** the requested level of help.

Arguments: `$ARGUMENTS`. The first token is the level (1, 2 or 3; default 1). The rest is the question or an issue number (`#12` → read it with `gh issue view 12`). If the question refers to code, read that code first.

## Levels

- **1 — направление.** One or two questions or observations that point toward the cause or approach ("кто владеет этим `CancellationTokenSource`?", "что произойдёт, если процесс упадёт между этими двумя строками?"). No API names, no code.
- **2 — инструменты.** Name the types, methods, patterns or spec sections to use and in what order. Link to docs (learn.microsoft.com) when useful. Still no code beyond a single identifier.
- **3 — минимальный пример.** The smallest code fragment that unblocks the user (≤ 15 lines), preferably in a neutral example rather than their exact code, plus a line-by-line explanation and the C++ analogue if there is one. The user still integrates it.

## Rules

- Answer in Russian.
- Never jump to a higher level than requested. At the end, offer the next level in one short line.
- If the user's approach is fundamentally off (contradicts the spec or will not work), say so at any level.
