using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Xml.XPath;
using AshMind.Extensions;
using HtmlAgilityPack;

namespace LightGet.Logic {
    public class LinkExtractor {
        public IEnumerable<Uri> ExtractLinks(Uri documentUrl, TextReader reader, string mediaType) {
            if (mediaType != MediaTypeNames.Text.Html && mediaType != "application/xhtml+xml")
                yield break;

            var document = new HtmlDocument();
            document.Load(reader);
            var xpath = document.CreateNavigator();

            var absoluteUriBase = documentUrl;
            var @base = (string)xpath.Evaluate("string(/html/head/base/@href)");
            if (@base.IsNotNullOrEmpty()) {
                // TODO: log this
                if (!Uri.TryCreate(@base, UriKind.Absolute, out absoluteUriBase))
                    absoluteUriBase = documentUrl;
            }

            var hrefs = xpath.Select("//a/@href").Cast<XPathNavigator>().Select(x => x.Value);
            foreach (var href in hrefs) {
                Uri uri;
                if (!Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out uri)) /* broken link */ { 
                    // TODO: log this
                    continue;
                }

                if (!uri.IsAbsoluteUri)
                    uri = new Uri(absoluteUriBase, uri);

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeFtp) {
                    // TODO: log this
                    continue;
                }

                yield return uri;
            }
        }
    }
}
