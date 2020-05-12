using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class CloneTests
    {
        [Fact]
        public void TestFromJson()
        {
            var config = new TestConfig();
            var dir = Guid.NewGuid().ToString("N");

            try
            {
                Clone.Run(config.Organization, config.Token, config.AreaPaths, dir)
                    .ToEnumerable()
                    .Should()
                    .NotBeEmpty();
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}