#!/usr/bin/env bash
# PostToolUse hook: after Claude edits a .cs file, normalize its whitespace
# (indentation, spacing, line endings) according to .editorconfig.
# Fast (--folder skips loading the MSBuild workspace) and never blocks.
set -u

input="$(cat)"
command -v jq >/dev/null 2>&1 || exit 0
command -v dotnet >/dev/null 2>&1 || exit 0

file="$(printf '%s' "$input" | jq -r '.tool_input.file_path // empty')"
[[ "$file" == *.cs ]] || exit 0

cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
rel="$(realpath --relative-to=. "$file" 2>/dev/null || printf '%s' "$file")"

dotnet format whitespace . --folder --include "$rel" >/dev/null 2>&1 || true
exit 0
