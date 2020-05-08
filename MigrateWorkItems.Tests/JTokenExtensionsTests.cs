using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class JTokenExtensionsTests
    {
        [Fact]
        public void ChangeDate()
        {
            var update = JToken.FromObject(new
            {
                fields = new Dictionary<string, object>
                {
                    ["System.ChangedDate"] = new
                    {
                        newValue = new DateTime(1999, 1, 3)
                    }
                }
            });

            update.ChangeDate()
                .Should()
                .Be(new DateTime(1999, 1, 3));
        }
        
        [Fact]
        public void CreatedDate()
        {
            var update = JToken.FromObject(new
            {
                fields = new Dictionary<string, object>
                {
                    ["System.CreatedDate"] = new
                    {
                        newValue = new DateTime(1999, 1, 3)
                    }
                }
            });

            update.ChangeDate()
                .Should()
                .Be(new DateTime(1999, 1, 3));
        }
        
        [Fact]
        public void RevisedDate()
        {
            var update = JToken.FromObject(new
            {
                revisedDate = new DateTime(1999, 1, 3)
            });

            update.ChangeDate()
                .Should()
                .Be(new DateTime(1999, 1, 3));
        }

        [Fact]
        public void Relations()
        {
            var update = JToken.FromObject(new
            {
            });

            update
                .Relations()
                .Should()
                .Be(0);
        }
        
        [Fact]
        public void RelationsAdded()
        {
            var update = JToken.FromObject(new
            {
                relations = new
                {
                    added = new[]
                    {
                        new
                        {
                            rel = "ArtifactLink"
                        }
                    }
                }
            });

            update
                .Relations()
                .Should()
                .Be(1);
        }
        
        [Fact]
        public void RelationsRemoved()
        {
            var update = JToken.FromObject(new
            {
                relations = new
                {
                    removed = new[]
                    {
                        new
                        {
                            rel = "ArtifactLink"
                        }
                    }
                }
            });

            update
                .Relations()
                .Should()
                .Be(1);
        }
    }
}