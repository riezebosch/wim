using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Flurl.Http;
using MigrateWorkItems.Model;

namespace MigrateWorkItems
{
    public static class Push
    {
        public static async IAsyncEnumerable<(int position, int total)> Run(string organization, string project, string output, MigrationContext context, WorkItemProcessor processor)
        {
            var query = context
                .Updates
                .AsQueryable()
                .Where(x => !x.Done)
                .OrderBy(x => x.ChangeDate)
                .ThenByDescending(x => x.RelationsRemoved)
                .ThenByDescending(x => x.RelationsAdded + x.RelationsRemoved);

            var position = 0;
            var total = query.Count();
            foreach (var item in query)
            {
                var update = Clone.FromFile(Path.Join(output, "items", item.WorkItemId.ToString(), item.Id + ".json"));

                try
                {
                    await processor.Process(organization, project, update);
                    item.Done = true;
                    context.Updates.Update(item);
                }
                catch (FlurlHttpException ex)
                {
                    throw new MigrationException($"Exception on work item {update.WorkItemId} with update {update.Id}")
                    {
                        Uri = ex.Call.Request.RequestUri,
                        Method = ex.Call.Request.Method,
                        Body = ex.Call.RequestBody,
                        StatusCode = ex.Call.Response.StatusCode,
                        Response = ex.Call.Response?.Content != null 
                                ? await ex.Call?.Response?.Content?.ReadAsStringAsync()
                                : string.Empty
                    };
                }

                await context.SaveChangesAsync();
                yield return (++position, total);
            }
        }
    }

    public class MigrationException : Exception
    {
        public MigrationException(string message) : base(message)
        {
        }

        public Uri Uri { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }
        public HttpMethod Method { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public override string ToString() => new StringBuilder()
            .AppendLine(Message)
            .AppendLine()
            .AppendLine($"{Method} {Uri}")
            .AppendLine()
            .AppendLine(StatusCode.ToString())
            .AppendLine()
            .AppendLine(Body)
            .AppendLine()
            .AppendLine(Response).ToString();
    }
}