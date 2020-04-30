using System;
using System.Collections.Generic;

namespace AzureDevOpsRest.Data.WorkItems
{
    public class WorkItem
    {
        public int Id { get; set; }
        public IDictionary<string, object> Fields { get; set; }
        public Uri Url { get; set; }
        public Relation[] Relations { get; set; }
    }
}