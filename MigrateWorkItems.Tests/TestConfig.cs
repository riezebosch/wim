using Microsoft.Extensions.Configuration;

namespace MigrateWorkItems.Tests
{
    public class TestConfig
    {
        public string Organization { get; set; }
        public string Token { get; set; }
        public string Project { get; set; }
        public string[] AreaPaths { get; set; }

        public TestConfig()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("settings.json", false)
                .AddJsonFile("settings.user.json", true)
                .Build();
            
            configuration.Bind(this);
        }
    }
}