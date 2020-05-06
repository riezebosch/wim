using System;
using FluentAssertions;
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

            description.TryGetAttachmentUrl(out var url, out var id, out var name)
                .Should()
                .BeTrue();
            
            url.Should()
                .Be("https://dev.azure.com/manuel/eb082604-a70f-4977-9335-85f0da463818/_apis/wit/attachments/f434ff22-1f72-4026-852d-a98470dba8b0?fileName=eenhoorn.png");
            
            id.Should()
                .Be("f434ff22-1f72-4026-852d-a98470dba8b0");

            name.Should()
                .Be("eenhoorn.png");
        }
        
        [Fact]
        public void NoMatch()
        {
            var description =
                "<div>hi</div>";

            description.TryGetAttachmentUrl(out var url, out var id, out var name)
                .Should()
                .BeFalse();
            
            url.Should()
                .Be(default);
            
            id.Should()
                .Be(Guid.Empty);

            name.Should().BeNull();
        }

        [Fact]
        public void Stop()
        {
            var description =
                "<div>Company details:</div><div><img src=\"https://dev.azure.com/unit4/81dea1dd-cac5-4d61-81a3-b2e7ff0c64bf/_apis/wit/attachments/f29f71de-117c-4c09-9602-14296d411f3a?fileName=image.png\"><br></div><div>Organization:</div><div><img src=\"https://dev.azure.com/unit4/81dea1dd-cac5-4d61-81a3-b2e7ff0c64bf/_apis/wit/attachments/f49f2db6-d418-4f30-b56c-abd8228b815c?fileName=image.png\"><br></div>";
            
            description.TryGetAttachmentUrl(out var url, out var id, out var name)
                .Should()
                .BeTrue();
            
            url.Should()
                .Be("https://dev.azure.com/unit4/81dea1dd-cac5-4d61-81a3-b2e7ff0c64bf/_apis/wit/attachments/f29f71de-117c-4c09-9602-14296d411f3a?fileName=image.png");
            
            id.Should()
                .Be("f29f71de-117c-4c09-9602-14296d411f3a");

            name.Should()
                .Be("image.png");
        }
    }
}