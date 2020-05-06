using System;
using System.Text.RegularExpressions;

namespace MigrateWorkItems
{
    public static class UrlExtensions
    {
        public static bool ToWorkItemId(this Uri target, out int id)
        {
            var match = Regex.Match(target.ToString(), @"(?<=https://dev.azure.com/.+/.+/_apis/wit/workItems/)\d+");
            return (id = match.Success ? int.Parse(match.Value) : 0) != 0;
        }
    }
}