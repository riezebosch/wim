using System;
using System.Threading.Tasks;
using AzureDevOpsRest;
using NSubstitute;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class CreateClassificationNodesTests
    {
        private readonly string _root;

        public CreateClassificationNodesTests()
        {
            _root = "for-the-test";
        }

        [Fact]
        public async Task CreateSingle()
        {
            var config = new TestConfig();
            var create = new CreateClassificationNodes(new Client(config.Token), config.Organization, config.Project);

            var area = Guid.NewGuid().ToString("N");
            await create.IfNotExists("areas", _root, area);
        }
        
        [Fact]
        public async Task CreateIfNotExists()
        {
            var config = new TestConfig();
            var create = new CreateClassificationNodes(new Client(config.Token), config.Organization, config.Project);

            var area = Guid.NewGuid().ToString("N");
            await create.IfNotExists("areas", _root, area);
            await create.IfNotExists("areas", _root, area);
        }
        
        [Fact]
        public async Task CreateNested()
        {
            var config = new TestConfig();
            var create = new CreateClassificationNodes(new Client(config.Token), config.Organization, config.Project);

            var parent = Guid.NewGuid().ToString("N");
            await create.IfNotExists("areas", _root, parent, "child");
        }
        
        [Fact]
        public async Task Cache()
        {
            var client = Substitute.For<IClient>();
            var create = new CreateClassificationNodes(client, "", "");

            var parent = Guid.NewGuid().ToString("N");
            await create.IfNotExists("areas", parent, "child");
            await create.IfNotExists("areas", parent, "child");

            await client.Received(2).GetAsync(Arg.Any<IRequest<object>>());
            await client.Received(2).PostAsync(Arg.Any<IRequest<object>>(), Arg.Any<object>());
        }
    }
}