using Grpc.Core;
using TaskFlow.Contracts.Grpc.V1;
using TaskFlow.Coordinator.Workers;

namespace TaskFlow.Coordinator.Services;

public class CoordinatorQueryService(IWorkerRegistry workerRegistry) : CoordinatorQuery.CoordinatorQueryBase
{
    public override Task<ListWorkersResponse> ListWorkers(ListWorkersRequest request,
        ServerCallContext context)
    {
        var response = new ListWorkersResponse();

        foreach (var worker in workerRegistry.GetAll())
        {
            response.Workers.Add(new WorkerSummary
            {
                WorkerId = worker.Id,
                WorkerName = worker.Name
            });
        }

        return Task.FromResult(response);
    }
}