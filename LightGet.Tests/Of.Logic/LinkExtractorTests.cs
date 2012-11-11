using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using LightGet.Logic;
using MbUnit.Framework;

namespace LightGet.Tests.Of.Logic {
    [TestFixture]
    public class LinkExtractorTests {
        [Test]
        [Row("http://x.com",    "http://x.com", "http://x.com/")]
        [Row("http://x.com/a/", "b",            "http://x.com/a/b")]
        [Row("http://x.com/a/", "/b",           "http://x.com/b")]
        [Row("https://x.com",   "a",            "https://x.com/a")]
        public void SimpleAHref(string url, string href, string expected) {
            var html = string.Format("<html><body><div><a href='{0}'>a</a></div></body></html>", href);
            var links = new LinkExtractor().ExtractLinks(new Uri(url), Read(html), MediaTypeNames.Text.Html);

            Assert.AreElementsEqualIgnoringOrder(new[] { expected }, links.Select(l => l.ToString()));
        }

        [Test]
        [Row("http://x.com",    "http://x.com", "http://x.com/")]
        [Row("http://x.com/a/", "b", "http://x.com/a/b")]
        [Row("http://x.com/a/", "/b", "http://x.com/b")]
        public void AHrefWithBase(string baseUrl, string href, string expected) {
            var html = string.Format("<html><head><base href='{0}'></head><body><div><a href='{1}'>a</a></div></body></html>", baseUrl, href);
            var links = new LinkExtractor().ExtractLinks(new Uri("http://y.com"), Read(html), MediaTypeNames.Text.Html);

            Assert.AreElementsEqualIgnoringOrder(new[] { expected }, links.Select(l => l.ToString()));
        }

        private TextReader Read(string html) {
            return new StringReader(html);
        }
    }
}
