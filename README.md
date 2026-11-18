# TaskFlow

Распределённый исполнитель bash-задач: web-UI → Coordinator → NATS JetStream → Worker-ы, состояние в PostgreSQL, наблюдаемость через OpenTelemetry → Grafana.

Учебный проект: изучение C#/.NET после C++ на системе, устроенной «почти как в проде» — несколько координаторов и воркеров, outbox, at-most-once/at-least-once, ретраи, отмена, live-логи, k8s.

## Документация

- **[Спецификация](docs/spec.md)** — архитектура, решения (ADR), схема БД, протокол, план работ. Источник истины.
- **Задачи** — [GitHub Issues](../../issues), по milestone на этап (M0 … M6, см. spec §12).
- **[Git workflow](docs/git-workflow.md)** — trunk-based: ветки, PR, squash merge, теги и контуры dev/prod.

## Стек

.NET 10 · ASP.NET Core (Minimal API, SSE) · Dapper + DbUp · PostgreSQL · NATS JetStream (`NATS.Net`) · OpenTelemetry → Grafana LGTM · Angular · Docker Compose · k3d (Kubernetes) · xUnit + Testcontainers

## Быстрый старт

Нужно: .NET SDK 10, Docker.

```bash
cp .env.example .env                                   # задать POSTGRES_PASSWORD
docker compose up postgres nats lgtm migrator          # инфраструктура; сервисы — из IDE
docker compose --profile full up --scale worker=3      # всё в контейнерах
```

| Что | Где |
|---|---|
| API | http://localhost:8080 (OpenAPI: `/openapi/v1.json`) |
| Grafana | http://localhost:3000 |
| NATS monitoring | http://localhost:8222 |

> Compose-файл приводится к этому виду в рамках этапа M0.

## Структура (после M0)

```
src/
  TaskFlow.Coordinator/     REST API, Dispatcher, Projector
  TaskFlow.Worker/          выполнение job-ов
  TaskFlow.Contracts/       сообщения NATS, имена subjects/streams
  TaskFlow.Migrator/        миграции БД + создание NATS streams/KV
  TaskFlow.ServiceDefaults/ OpenTelemetry, health checks
tests/
ui/taskflow-ui/
deploy/
docs/
```

## Работа с Claude

В репозитории лежит конфигурация для Claude Code (в т.ч. плагина для JetBrains Rider):

- [`CLAUDE.md`](CLAUDE.md) — контекст проекта и правила: Claude по умолчанию ментор и ревьюер, код пишет автор.
- `.claude/skills/` — команды:
  - `/brief <issue>` — разбор задачи: план, что изучить, подводные камни, как проверить
  - `/hint <1|2|3> <вопрос>` — подсказка нужного уровня, без готового решения
  - `/mentor-review [путь|ветка]` — ревью кода (C#-идиомы, привычки из C++, соответствие спеке)
  - `/explain-cs <понятие>` — объяснение концепции C# через C++
  - `/spec-update <изменение>` — согласованная правка спеки с версией и changelog
- `.claude/settings.json` — разрешения и hooks:
  - запрет правки уже закоммиченных миграций (они append-only);
  - форматирование `.cs`-файлов после правок Claude (`dotnet format whitespace`).

Hooks используют `jq`; без него они просто ничего не делают.

Личные настройки — в `.claude/settings.local.json` и `CLAUDE.local.md` (в `.gitignore`).
