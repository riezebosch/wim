using System.Collections.Generic;
using AzureDevOpsRest.Data.WorkItems;

namespace MigrateWorkItems.Data
{
    public class Relations
    {
        public IEnumerable<Relation> Added { get; set; }
        public IEnumerable<Relation> Removed { get; set; }
    }
}