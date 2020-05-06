using System;

namespace MigrateWorkItems.Data
{
    public class WorkItemField
    {
        public string ReferenceName { get; set; }
        public Uri Url { get; set; }
        public bool ReadOnly { get; set; }
    }
}