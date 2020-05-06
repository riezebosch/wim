using System;

namespace MigrateWorkItems.Model
{
    public class Update
    {
        public int Id { get; set; }
        public int WorkItemId { get; set; }
        public DateTime ChangeDate { get; set; }
        public bool Done { get; set; }
        public int Relations { get; set; }
    }
}