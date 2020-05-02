using System;

namespace MigrateWorkItems.Tests.Data
{
    public class WorkItemField
    {
        public string ReferenceName { get; set; }
        public Uri Url { get; set; }
        public bool ReadOnly { get; set; }
    }
}