using System.Linq;
using AzureDevOpsRest;
using FluentAssertions;
using MigrateWorkItems.Save;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class DownloadTests
    {
        [Fact]
        public void To()
        {
            var config = new TestConfig();
            var client = new Client(config.Token);

            var download = new Download(client);

            download.To(config.Organization, config.Project)
                .ToEnumerable()
                .Should()
                .NotBeEmpty();
        }
    }
}