using System.IO;
using Newtonsoft.Json.Linq;

namespace MigrateWorkItems.Save
{
    public class SaveWorkItems
    {
        private readonly DirectoryInfo _items;

        public SaveWorkItems(DirectoryInfo items) => this._items = items;

        public JToken To(JToken update)
        {
            var item = _items.CreateSubdirectory(update.SelectToken("workItemId").Value<string>());
            var path = Path.Combine(item.FullName, update.SelectToken("id").Value<string>() + ".json");
            File.WriteAllText(path, update.ToString());

            return update;
        }
    }
}