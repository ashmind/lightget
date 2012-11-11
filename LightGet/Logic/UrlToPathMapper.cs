using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AshMind.Extensions;

namespace LightGet.Logic {
    public class UrlToPathMapper {
        private readonly Uri relativeTo;
        private readonly PathMappingRule rule;

        private static readonly Regex SanitizePattern = new Regex("[" +
            Regex.Escape(new string(Path.GetInvalidPathChars()) + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar)
        + "]");

        public UrlToPathMapper(PathMappingRule rule, Uri relativeTo = null) {
            this.rule = rule;
            if (this.rule.IncludePath && !this.rule.IncludeParentPath && relativeTo == null)
                throw new ArgumentNullException("relativeTo", "Relative url must be set if the mapping rule requires relative paths.");

            this.relativeTo = relativeTo;
        }

        public string GetPath(Uri url, string fileName = null) {
            if (!url.IsAbsoluteUri)
                throw new ArgumentException("Url must be absolute.", "url");

            var result = new StringBuilder();
            if (rule.IncludeHost)
                result.Append(url.Host);

            if (!url.IsDefaultPort && rule.IncludePort)
                result.Append("_").Append(url.Port);

            if (url.LocalPath != "/" && rule.IncludePath) {
                var path = url.LocalPath.Replace('/', Path.DirectorySeparatorChar);
                if (!rule.IncludeParentPath) {
                    var rootPath = this.relativeTo.LocalPath.Replace('/', Path.DirectorySeparatorChar);
                    path = path.RemoveStart(rootPath);
                }

                result.Append(path);
            }

            if (result.Length > 0 && result[0] == Path.DirectorySeparatorChar)
                result.Remove(0, 1);

            // ignore query if we have a file name
            if (fileName != null) {
                var path = result.ToString();
                // also replace last part of path with the file name
                return Path.Combine(Path.GetDirectoryName(path), fileName);
            }

            var pathIsFile = !url.LocalPath.EndsWith("/"); // heuristics
            var resultNeedsSeparator = result.Length > 0 && result[result.Length - 1] != Path.DirectorySeparatorChar;
            
            var query = url.Query.TrimStart('?');
            var hasQuery = query.IsNotNullOrEmpty();
            if (hasQuery) {
                if (!pathIsFile && resultNeedsSeparator)
                    result.Append(Path.DirectorySeparatorChar);

                result.Append("_").Append(Sanitize(query));
            }

            // if we think this is a folder and query did not create a 'file name'
            // then 'file name' will be '_'
            if (!hasQuery && !pathIsFile) {
                if (resultNeedsSeparator)
                    result.Append(Path.DirectorySeparatorChar);

                result.Append("_");
            }

            return result.ToString();
        }

        private string Sanitize(string part) {
            return SanitizePattern.Replace(part, "_");
        }
    }
}
