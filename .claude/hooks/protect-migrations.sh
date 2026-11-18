#!/usr/bin/env bash
# PreToolUse hook: DbUp migrations are append-only.
# Blocks Edit/Write of a migration script that is already tracked by git;
# creating a new NNN_*.sql file is allowed.
# Exit 2 = block the tool call; stderr is shown to Claude.
set -u

input="$(cat)"
command -v jq >/dev/null 2>&1 || exit 0

file="$(printf '%s' "$input" | jq -r '.tool_input.file_path // empty')"
[[ -n "$file" ]] || exit 0
[[ "$file" == *.sql && "$file" == */Scripts/* ]] || exit 0

cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
rel="$(realpath --relative-to=. "$file" 2>/dev/null || printf '%s' "$file")"

if git ls-files --error-unmatch -- "$rel" >/dev/null 2>&1; then
  echo "Migration '$rel' is already committed. DbUp migrations are append-only: add a new NNN_description.sql script instead of editing this one." >&2
  exit 2
fi

exit 0
