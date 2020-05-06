using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems.Data
{
    public class WorkItemUpdate
    {
        public IDictionary<string, Value> Fields { get; set; }
        public Relations Relations { get; set; }
        public DateTime RevisedDate { get; set; }
        public int Id { get; set; }
        public int Rev { get; set; }
        public JToken RevisedBy { get; set; }
        public int WorkItemId { get; set; }
    }
}