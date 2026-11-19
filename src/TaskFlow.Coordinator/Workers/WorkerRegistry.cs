using System.Collections.Concurrent;

namespace TaskFlow.Coordinator.Workers;

public sealed class WorkerRegistry : IWorkerRegistry
{
  private readonly ConcurrentDictionary<KeyValuePair<Guid, string>, WorkerSession> _workers = new();

  public bool Register(WorkerSession session)
  {
    var key = new KeyValuePair<Guid, string>(session.Id, session.WorkerInfo.Id);
    return _workers.TryAdd(key, session);
  }

  public bool Unregister(string workerId, Guid sessionId)
  {
    var key = new KeyValuePair<Guid, string>(sessionId, workerId);
    return _workers.TryRemove(key, out _);
  }

  public IReadOnlyCollection<WorkerInfo> GetAll()
  {
    return [.. _workers.Values.Select(x => x.WorkerInfo)];
  }
}
