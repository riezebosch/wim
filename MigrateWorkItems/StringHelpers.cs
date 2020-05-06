using System;
using System.Text.RegularExpressions;

namespace MigrateWorkItems
{
    public static class StringHelpers
    {
        public static bool TryGetAttachmentUrl(this string description, out Uri url, out Guid id, out string name)
        {
            var match = Regex.Match(description, @"https://dev\.azure\.com/.+?/.+?/_apis/wit/attachments/(?<id>[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12})\?fileName=(?<filename>[^\""]+)");
            if (match.Success)
            {
                url = new Uri(match.Value);
                id = new Guid(match.Groups["id"].Value);
                name = match.Groups["filename"].Value;
            
                return true;
            }

            url = null;
            id = Guid.Empty;
            name = null;
            
            return false;
        }
    }
}