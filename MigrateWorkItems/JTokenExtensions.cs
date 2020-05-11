using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems
{
    public static class JTokenExtensions
    {
        public static DateTime ChangeDate(this JToken update)
        {
            return (DateTime?)update.SelectToken("fields.['System.ChangedDate'].newValue")
                   ?? (DateTime?)update.SelectToken("fields.['System.CreatedDate'].newValue")
                   ?? (DateTime)update.SelectToken("revisedDate");
        }

        public static int RelationsAdded(this JToken update) =>
            update.SelectTokens("relations.added[*]").Count();
        
        public static int RelationsRemoved(this JToken update) =>
            update.SelectTokens("relations.removed[*]").Count();
    }
}