using AzureDevOpsRest.Data.Release;

namespace AzureDevOpsRest.Requests
{
    public static class Build
    {
        public static IRequest<Definition> Definition(string organization, string project, int id) => 
            new Request<Definition>($"{organization}/{project}/_apis/build/definitions/{id}", "5.1");

        public static IEnumerableRequest<Definition> Definitions(string organization, string project) => 
            new Request<Definition>($"{organization}/{project}/_apis/build/definitions/", "5.1").AsEnumerable();

        public static IEnumerableRequest<Data.Build.Build> Builds(string organization, string project) =>
            new Request<Data.Build.Build>($"{organization}/{project}/_apis/build/builds/", "5.1").AsEnumerable();
    }
}