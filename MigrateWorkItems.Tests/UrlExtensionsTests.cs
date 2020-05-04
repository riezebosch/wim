using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public static class UrlExtensionsTests
    {
        [Fact]
        public static void ToId()
        {
            var target =
                new Uri("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/2195");

            target
                .ToWorkItemId(out var id)
                .Should()
                .BeTrue();
            id
                .Should()
                .Be(2195);
        }
        
        [Fact]
        public static void FromUpdate()
        {
            var target =
                new Uri("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/workItems/2195/updates/1");

            target
                .ToWorkItemId(out var id)
                .Should()
                .BeTrue();
            id
                .Should()
                .Be(2195);
        }
        
        [Fact]
        public static void NoMatch()
        {
            var target =
                new Uri("https://www.google.com");

            target.ToWorkItemId(out _)
                .Should()
                .BeFalse();
        }
    }
}