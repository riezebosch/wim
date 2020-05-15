using System;
using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class CloneTests
    {
        [Fact]
        public async Task TestFromJson()
        {
            var config = new TestConfig();
            var dir = Guid.NewGuid().ToString("N");

            try
            {
                var progress = Substitute.For<IProgress<Clone.Progress>>();
                await Clone.Run(config.Organization, config.Token, config.AreaPaths, dir, progress);
                
                progress
                    .Received()
                    .Report(Arg.Any<Clone.Progress>());
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}