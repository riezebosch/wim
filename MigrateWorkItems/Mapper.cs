using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MigrateWorkItems.Model;

namespace MigrateWorkItems
{
    public class Mapper : IMapper
    {
        private readonly MigrationContext _context;

        public Mapper(MigrationContext context) => _context = context;

        public async Task WorkItem(int from, Uri to)
        {
            _context.WorkItemMapping.Add(new WorkItemMapping {Id = from, Url = to});
            await _context.SaveChangesAsync();
        }

        public bool TryGetWorkItem(int id, out Uri url)
        {
            var item = _context.WorkItemMapping
                .AsNoTracking()
                .SingleOrDefault(x => x.Id == id);
            return (url = item?.Url) != null;
        }
    }
}