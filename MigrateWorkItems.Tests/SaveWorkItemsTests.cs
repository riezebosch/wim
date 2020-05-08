using System;
using System.IO;
using FluentAssertions;
using MigrateWorkItems.Save;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class SaveWorkItemsTests
    {
        [Fact]
        public void To()
        {
            var items = Directory.CreateDirectory("test").CreateSubdirectory(Guid.NewGuid().ToString());
            var save = new SaveWorkItems(items);

            save.To(JToken.FromObject(new
                {
                    id = 1,
                    workItemId = 12
                }));

            File
                .Exists(Path.Join(items.FullName, "12", "1.json"))
                .Should()
                .BeTrue();
        }
    }
}