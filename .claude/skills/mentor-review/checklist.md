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
- Lifetimes: a singleton does not capture a scoped service (e.g. `DbContext`-like or per-request objects).
- `BackgroundService.ExecuteAsync` loops honor `stoppingToken` and do not crash the host on one failure (log + backoff).
- Options validated on start (`ValidateDataAnnotations().ValidateOnStart()` or `IValidateOptions`).

## Data (Dapper / Postgres)
- Parameters, never string-concatenated SQL.
- Statements that must be atomic share one `NpgsqlTransaction`; the transaction is passed to every Dapper call.
- `FOR UPDATE SKIP LOCKED` batches are short; network calls inside a transaction have a timeout (spec §5).
- Migrations are new append-only scripts.

## NATS / messaging (spec §4, §6.2)
- Ack/Nak/Term only **after** the event about the job got its PubAck.
- `Nats-Msg-Id` set where deduplication is expected.
- Subject/stream names come from `TaskFlow.Contracts`.
- Projector work is idempotent (inbox) and respects the `attempt` fencing token.

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
