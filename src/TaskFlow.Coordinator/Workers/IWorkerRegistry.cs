namespace TaskFlow.Coordinator.Workers;

public interface IWorkerRegistry
{
    bool Register(WorkerSession session);
    bool Unregister(string workerId, Guid sessionId);

    IReadOnlyCollection<WorkerInfo> GetAll();
}