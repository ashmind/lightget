using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AshMind.Extensions;

namespace LightGet.Logic {
    public class UrlToPathMapper {
        private static readonly Regex SanitizePattern = new Regex("[" +
            Regex.Escape(new string(Path.GetInvalidPathChars()) + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar)
        + "]");
        
        public string GetPath(Uri url, string fileName = null) {
            if (!url.IsAbsoluteUri)
                throw new ArgumentException("Url must be absolute.", "url");
            
            var result = new StringBuilder(url.Host);
            if (!url.IsDefaultPort)
                result.Append("_").Append(url.Port);

            if (url.LocalPath != "/")
                result.Append(url.LocalPath.Replace('/', Path.DirectorySeparatorChar));

            // ignore query if we have a file name
            if (fileName != null) {
                var path = result.ToString();
                // also replace last part of path with the file name
                return Path.Combine(Path.GetDirectoryName(path), fileName);
            }

            var pathIsFile = !url.LocalPath.EndsWith("/"); // heuristics
            
            var query = url.Query.TrimStart('?');
            var hasQuery = query.IsNotNullOrEmpty();
            if (hasQuery) {
                if (!pathIsFile)
                    result.Append(Path.DirectorySeparatorChar);

                result.Append("_").Append(Sanitize(query));
            }

            // if we think this is a folder and query did not create a 'file name'
            // then 'file name' will be '_'
            if (!hasQuery && !pathIsFile)
                result.Append(Path.DirectorySeparatorChar).Append("_");

            return result.ToString();
        }

        private string Sanitize(string part) {
            return SanitizePattern.Replace(part, "_");
        }
    }
}
