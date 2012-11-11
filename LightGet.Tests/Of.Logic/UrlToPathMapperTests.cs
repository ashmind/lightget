using System;
using System.Collections.Generic;
using System.Linq;
using LightGet.Logic;
using MbUnit.Framework;

namespace LightGet.Tests.Of.Logic {
    [TestFixture]
    public class UrlToPathMapperTests {
        [Test]
        [Row("http://x.com",            @"x.com\_")]
        [Row("http://x.com/a/b/c",      @"x.com\a\b\c")]
        [Row("http://x.com?a=b",        @"x.com\_a=b")]
        [Row("http://x.com?a=b&c=d",    @"x.com\_a=b&c=d")]
        [Row("http://x.com?a=b/c",      @"x.com\_a=b_c")]
        [Row("http://x.com/a?b=c",      @"x.com\a_b=c")]
        [Row("http://x.com:8080?a=b/c", @"x.com_8080\_a=b_c")]
        [Row("http://x.com:8080/a?b=c", @"x.com_8080\a_b=c")]
        public void Full(string uri, string expected) {
            var mapped = new UrlToPathMapper(new PathMappingRule(), new Uri(uri)).GetPath(new Uri(uri));
            Assert.AreEqual(expected, mapped);
        }

        [Test]
        [Row("http://x.com",            @"_")]
        [Row("http://x.com/a",          @"a")]
        [Row("http://x.com/a/",         @"a\_")]
        [Row("http://x.com/a/b/c",      @"a\b\c")]
        [Row("http://x.com?a=b",        @"_a=b")]
        [Row("http://x.com?a=b/c",      @"_a=b_c")]
        [Row("http://x.com/a?b=c",      @"a_b=c")]
        public void FullPath(string uri, string expected) {
            var rule = new PathMappingRule {
                IncludeHost = false,
                IncludePort = false
            };
            var mapped = new UrlToPathMapper(rule, new Uri(uri)).GetPath(new Uri(uri));
            Assert.AreEqual(expected, mapped);
        }

        [Test]
        [Row("http://x.com/a/", "http://x.com/a/",   @"_")]
        [Row("http://x.com/a/", "http://x.com/a/b",  @"b")]
        [Row("http://x.com/a/", "http://x.com/a/b/", @"b\_")]
        public void Relative(string parentUri, string uri, string expected) {
            var rule = new PathMappingRule {
                IncludeHost = false,
                IncludePort = false,
                IncludePath = true,
                IncludeParentPath = false
            };
            var mapped = new UrlToPathMapper(rule, new Uri(parentUri)).GetPath(new Uri(uri));
            Assert.AreEqual(expected, mapped);
        }

        [Test]
        [Row("http://x.com/a?k=p",  "something.zip", @"x.com\something.zip")]
        [Row("http://x.com/a",      "something.zip", @"x.com\something.zip")]
        [Row("http://x.com/a/",     "something.zip", @"x.com\a\something.zip")]
        [Row("http://x.com/a/b",    "something.zip", @"x.com\a\something.zip")]
        public void FullWithFileName(string uri, string fileName, string expected) {
            var mapped = new UrlToPathMapper(new PathMappingRule(), new Uri(uri)).GetPath(new Uri(uri), fileName);
            Assert.AreEqual(expected, mapped);
        }
    }
}
