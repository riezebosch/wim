using AzureDevOpsRest.Data.WorkItems;

namespace AzureDevOpsRest.Requests
{
    public static class WorkItems
    {
        public static Request<WorkItemQueryResult> Query(string organization) =>
            new Request<WorkItemQueryResult>($"{organization}/_apis/wit/wiql", "5.1");

        public static IRequest<WorkItem> WorkItem(string organization, int id, params string[] fields) => 
            new Request<WorkItem>($"{organization}/_apis/wit/workitems", "5.1")
                .WithQueryParams(("id", id))
                .WithQueryParams(("fields", string.Join(",", fields)));
    }
}