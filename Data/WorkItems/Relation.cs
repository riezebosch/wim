using System;
using Newtonsoft.Json.Linq;

namespace AzureDevOpsRest.Data.WorkItems
{
    public class Relation
    {
        public string Rel { get; set; }
        public Uri Url { get; set; }
        public JToken Attributes { get; set; }
    }
}