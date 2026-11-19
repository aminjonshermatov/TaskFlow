# Git workflow

Trunk-based development: одна долгоживущая ветка `master` и короткоживущие ветки задач. Контуры (dev/prod) задаются окружениями и тегами, а не ветками.

## Ветки

- `master` — всегда собирается, тесты зелёные, можно выкатывать. Прямой push запрещён.
- Ветка задачи живёт от нескольких часов до пары дней: `<тип>/<номер-issue>-<описание>`.

| Тип | Когда | Пример |
|---|---|---|
| `feat` | новая функциональность | `feat/10-migrator-log-stream` |
| `fix` | исправление ошибки | `fix/31-dispatcher-timeout` |
| `refactor` | изменение структуры без изменения поведения | `refactor/13-remove-grpc-worker` |
| `test` | только тесты | `test/18-integration-fixture` |
| `docs` | документация, спека | `docs/spec-v1.1-git-flow` |
| `chore` | сборка, CI, зависимости, инфраструктура | `chore/ci-build-test` |

Без issue (мелкая правка) — номер опускается: `fix/typo-readme`.

## Issue и PR

PR — это одно связное изменение, которое можно отревьюить и после которого `master` остаётся рабочим.

| Ситуация | Как |
|---|---|
| Обычная задача | 1 issue → 1 PR, `Closes #N` |
| Большая задача | несколько PR с `Part of #N`, последний — `Closes #N` |
| Пара крошечных связанных задач | один PR, `Closes #N, closes #M` |
| Мелкая находка по ходу | отдельный маленький PR, issue не обязателен |

Ориентир по размеру: PR читается за 15–20 минут. Если нет — разбить.

Изменения спеки (`docs/spec.md`) — отдельными PR с типом `docs`, чтобы решения не терялись внутри PR с кодом.

## Коммиты

- Conventional Commits: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`; можно со scope: `feat(worker): …`.
- Внутри ветки коммить как удобно — история ветки схлопнется при merge.
- Никогда не коммитить `.env`, секреты, `.claude/settings.local.json`.

## Цикл работы над задачей

```bash
git switch master && git pull
git switch -c feat/10-migrator-log-stream
# … работа, коммиты …
git fetch && git rebase origin/master          # подтянуть свежий master
git push -u origin HEAD                      # открыть PR (можно draft)
```

1. В Claude: `/brief 16` — разбор задачи перед началом.
2. Draft PR открывать рано: CI и `/mentor-review` смотрят код до готовности.
3. Перед merge: зелёный CI, `/mentor-review` без 🔴, чек-лист шаблона PR заполнен.
4. **Squash and merge.** Заголовок PR становится сообщением коммита в `master`, поэтому он в формате Conventional Commits.
5. Ветку удалить (GitHub может делать это автоматически).

## Настройки репозитория на GitHub

- **Settings → General → Pull Requests:** оставить только *Allow squash merging* (default message: *Pull request title and description*); включить *Automatically delete head branches*.
- **Settings → Rules → Rulesets** для `master`:
  - *Restrict deletions*, *Block force pushes*;
  - *Require a pull request before merging*, required approvals = 0 (свой PR одобрить нельзя);
  - *Require status checks to pass* — проверки CI (добавить, когда появится workflow);
  - *Require branches to be up to date before merging*;
  - *Require linear history*.

## Версии и контуры

| Что | Как помечается | Куда выкатывается |
|---|---|---|
| Каждый merge в `master` | образы `:master`, `:sha-<короткий-sha>` | **dev**-контур |
| Релиз | тег `vX.Y.Z` на коммите `master` | **prod**-контур |
| Версия спеки | тег `spec-vX.Y` | — |

Версии кода по этапам: M1 → `v0.1.0`, …, M6 → `v0.6.0`, весь MVP → `v1.0.0`. Исправление в релизе → patch (`v0.1.1`). Теги спеки и кода независимы.

Откат prod — выкатить предыдущий тег.

## CI/CD

Устройство CI, публикации образов и выкатки через Argo CD — spec §10.5. Workflow-ы — в `.github/workflows/`:

- `ci.yml` — на каждый PR и push в `master`: build, format, тесты, сборка образов. Обязательная проверка для merge.
- `publish.yml` — после merge в `master`: образы `:master`, `:sha-*` в GHCR → Argo CD выкатывает в **dev**.
- `release.yml` — на тег `vX.Y.Z`: образы `:X.Y.Z` и PR с новой версией в `overlays/prod` → после merge Argo CD выкатывает в **prod**.

Релиз:

```bash
git switch master && git pull
git tag -a v0.1.0 -m "M1: happy path"
git push origin v0.1.0        # → release.yml → PR в overlays/prod → merge → prod
```
