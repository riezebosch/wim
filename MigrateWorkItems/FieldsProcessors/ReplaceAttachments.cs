using System.Threading.Tasks;
using MigrateWorkItems.Data;

namespace MigrateWorkItems.FieldsProcessors
{
    public class ReplaceAttachments : IFieldsProcessor
    {
        private readonly IMapper _mapper;

        public ReplaceAttachments(IMapper mapper) => _mapper = mapper;

        public Task Execute(WorkItemUpdate item)
        {
            if (item.Fields != null && item.Fields.TryGetValue("System.Description", out var field))
            {
                foreach (var attachment in ((string) field.NewValue).GetAttachments())
                {
                    if (_mapper.TryGetAttachment(attachment.Id, out var to))
                    {
                        field.NewValue = ((string) field.NewValue).Replace(attachment.Url.ToString(), to.ToString());
                    }
                }
            }
            
            return Task.CompletedTask;
        }
    }
}