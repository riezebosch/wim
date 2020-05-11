using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MigrateWorkItems.Data;
using MigrateWorkItems.Model;

namespace MigrateWorkItems
{
    public static class StringHelpers
    {
        public static IEnumerable<AttachmentReference> GetAttachments(this string description)
        {
            var matches = Regex.Matches(description, @"https://dev\.azure\.com/.+?/.+?/_apis/wit/attachments/(?<id>[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})\?fileName=[^\""]+");
            foreach (Match match in matches)
            {
                yield return new AttachmentReference
                {
                    Id = new Guid(match.Groups["id"].Value),
                    Url = new Uri(match.Value)
                };
            }
        }
    }
}