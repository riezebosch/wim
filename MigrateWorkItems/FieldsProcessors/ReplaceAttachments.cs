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
            if (item.Fields != null 
                && item.Fields.TryGetValue("System.Description", out var field)
                && ((string) field.NewValue).TryGetAttachmentUrl(out var uri, out var id, out _)
                && _mapper.TryGetAttachment(id, out var to))
            {
                field.NewValue = ((string) field.NewValue).Replace(uri.ToString(), to.ToString());
            }
            
            return Task.CompletedTask;
        }
    }
}