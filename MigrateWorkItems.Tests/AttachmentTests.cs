using System;
using FluentAssertions;
using MigrateWorkItems.Model;
using Xunit;

namespace MigrateWorkItems.Tests
{
    public class AttachmentTests
    {
        [Fact]
        public void Extract()
        {
            var description =
                "<div><img src=\"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png\" alt=eenhoorn.png><br></div>";

            description.GetAttachments()
                .Should()
                .BeEquivalentTo(
                    new AttachmentMapping
                    {
                        Id = new Guid("f434ff22-1f72-4026-852d-a98470dba8b0"),
                        Url = new Uri(
                            "https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png")
                    });
        }
        
        [Fact]
        public void Mutiple()
        {
            var description =
                "<div>" +
                "   <img src=\"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png\" alt=eenhoorn.png><br>" +
                "   <img src=\"https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f04713ec-c2cf-4bc9-ae27-fe3fe26995d2?fileName=twee.png\" alt=twee.png><br>" +
                "</div>";

            description.GetAttachments()
                .Should()
                .BeEquivalentTo(
                    new AttachmentMapping
                    {
                        Id = new Guid("f434ff22-1f72-4026-852d-a98470dba8b0"),
                        Url = new Uri(
                            "https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png")
                    },
                    new AttachmentMapping
                    {
                        Id = new Guid("f04713ec-c2cf-4bc9-ae27-fe3fe26995d2"),
                        Url = new Uri(
                            "https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f04713ec-c2cf-4bc9-ae27-fe3fe26995d2?fileName=twee.png")
                    });
        }
        
        [Fact]
        public void NoMatch()
        {
            var description =
                "<div>hi</div>";

            description.GetAttachments()
                .Should()
                .BeEmpty();
        }
    }
}