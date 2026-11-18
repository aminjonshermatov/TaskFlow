---
name: explain-cs
description: Explain a C#/.NET concept, keyword, API or runtime behavior through its closest C++ analogue and the differences. Use when the user asks what something in C# is or how it works ("что такое", "как работает", "в чём разница", "зачем нужен").
argument-hint: "[concept]"
---

# Explain C# through C++

Explain `$ARGUMENTS` (or the concept from the conversation) to an experienced C++ developer who is new to C#. Answer in Russian.

Structure:

1. **Коротко** — what it is, in one or two sentences.
2. **Аналог в C++** — the closest C++ construct (or "прямого аналога нет" and why).
3. **Чем отличается** — the differences that will bite a C++ developer: memory/GC, value vs reference semantics, exceptions, threading, what the compiler or runtime generates.
4. **Пример** — a small self-contained snippet (≤ 20 lines), ideally side by side with C++.
5. **Где это в TaskFlow** — where the concept appears or will appear in the project (spec section or file), if anywhere.
6. **Подводные камни** — 1–3 common mistakes.

Keep it tight. Prefer precise statements over analogies that are only roughly true; when an analogy breaks, say where.
