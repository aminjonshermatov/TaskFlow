# Review checklist for TaskFlow (C# for a C++ developer)

## async / cancellation
- No `.Result`, `.Wait()`, `GetAwaiter().GetResult()` on tasks (deadlocks, thread-pool starvation).
- No `async void` except event handlers.
- Every async method takes a `CancellationToken` and passes it to every call that accepts one.
- Linked `CancellationTokenSource` for timeouts (`CreateLinkedTokenSource` + `CancelAfter`), disposed with `using`.
- `OperationCanceledException` caught only when the token that was cancelled is ours; not swallowed otherwise.
- `ValueTask` is awaited exactly once; not stored and awaited twice.
- Fire-and-forget tasks are observed (stored and awaited on shutdown), not dropped.

## Resources (RAII → `using`)
- `IDisposable` / `IAsyncDisposable` objects are in `using` / `await using`: connections, transactions, CTS, `Process`, streams.
- No finalizers or manual `Dispose()` calls where `using` works.

## Nullability and types
- No `!` (null-forgiving) without a comment explaining why it is safe.
- `record` for immutable data / messages; `class` for services; `struct` only with a measured reason.
- `required` / `init` instead of half-initialized objects.

## Errors
- Exceptions for exceptional cases, not error codes or `bool` + out params by default.
- No empty `catch`; no `catch (Exception)` without rethrow or a clear reason.
- `throw;` not `throw ex;` (keeps the stack trace).

## Concurrency
- Shared mutable state is avoided; if needed — `Channel<T>`, `ConcurrentDictionary`, or `lock` on a private object, never `lock(this)`.
- No `Thread.Sleep` in async code — `await Task.Delay(…, ct)` or `TimeProvider`.
- No blocking calls inside `lock`; no `await` inside `lock` (does not compile — but watch for `Monitor`/`SemaphoreSlim` misuse).

## DI and hosting
- Lifetimes: a singleton does not capture a scoped service (`DbContext`, per-request objects); in a `BackgroundService` use `IServiceScopeFactory` and a scope per unit of work.
- `BackgroundService.ExecuteAsync` loops honor `stoppingToken` and do not crash the host on one failure (log + backoff).
- Options validated on start (`ValidateDataAnnotations().ValidateOnStart()` or `IValidateOptions`).

## Data (EF Core / Postgres, spec §5)
- `DbContext` is scoped and never shared between threads or concurrent operations; no `DbContext` in a singleton.
- Read-only queries use `AsNoTracking()`; no N+1 (lazy loading off, explicit `Include`/projection).
- Raw SQL only through `FromSql`/`SqlQuery`/`ExecuteSql` with interpolated parameters — never `FromSqlRaw` with string concatenation.
- Work that must be atomic shares one `SaveChangesAsync` or one explicit transaction; the bus outbox writes into the same `DbContext`.
- `FOR UPDATE SKIP LOCKED` batches are short; no network calls inside a transaction (publish goes to the outbox).
- Bulk changes use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` instead of loading entities.
- Migrations are new and append-only; the model snapshot matches the model (`dotnet ef migrations has-pending-model-changes`).

## MassTransit / RabbitMQ (spec §4, §6.2, ADR-10)
- `run-job` endpoint: no `UseMessageRetry`, no `UseDelayedRedelivery`; `PrefetchCount = 1`, `ConcurrentMessageLimit = 1`.
- A `RunJob` with `ReceiveContext.Redelivered == true` is never executed — only `AttemptLost` is published.
- Events carry `attempt`; the saga ignores events whose attempt differs from `CurrentAttempt` (fencing).
- State changes and outgoing messages go through the EF Core outbox in one transaction; inbox is enabled on saga/consumer endpoints.
- Consumers pass `context.CancellationToken` down; no fire-and-forget `Publish` without `await`.
- Message types are records from `TaskFlow.Contracts`; renaming a type or namespace is a breaking change (exchange names).
- Saga repository: EF Core, pessimistic concurrency, Postgres lock statements.
- Job logs go to the RabbitMQ stream via `RabbitMQ.Stream.Client`, not through MassTransit.

## Logging and telemetry
- Structured templates (`"Job {JobId} started"`), not string interpolation in log calls.
- `LoggerMessage` source generator for hot paths (optional, learning point).
- Activities/metrics follow names in spec §9.

## Time and IDs
- `TimeProvider`, never `DateTime.Now/UtcNow` in logic.
- `Guid.CreateVersion7()` for new IDs.

## Tests
- Behavior tested, not implementation details.
- Time-dependent logic tested with `FakeTimeProvider`, no real sleeps.
- Integration tests use Testcontainers and clean state per test.
