using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MigrateWorkItems.Tests
{
    public class CloneTests
    {
        private readonly ITestOutputHelper _output;

        public CloneTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task TestFromJson()
        {
            var config = new TestConfig();
            var dir = Guid.NewGuid().ToString("N");

            try
            {
                await Clone.RunClone(config.Organization, config.Token, config.AreaPaths, dir, _output.WriteLine);
            }
            finally
            {
                Directory.Delete(dir, true);
            }

        }
    }
}