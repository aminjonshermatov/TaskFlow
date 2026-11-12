namespace TaskFlow.Coordinator.Jobs;

public enum JobStatus
{
    Pending,
    Running,
    Canceled,
    Timeout,
    Success,
    Failed
}