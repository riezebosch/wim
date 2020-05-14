namespace AzureDevOpsRest.Requests
{
    public static class RequestExtensions
    {
        public static IEnumerableRequest<TData> AsTopSkipEnumerable<TData>(this IRequest<TData> request) =>
            new TopSkipEnumerator<TData>(request);
    }
}