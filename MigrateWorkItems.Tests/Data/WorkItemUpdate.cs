using System;
using System.Collections.Generic;

namespace MigrateWorkItems.Tests.Data
{
    public class WorkItemUpdate
    {
        public IDictionary<string, Value> Fields { get; set; }
        public Relations Relations { get; set; }
        public DateTime RevisedDate { get; set; }
        public int Id { get; set; }
    }
}