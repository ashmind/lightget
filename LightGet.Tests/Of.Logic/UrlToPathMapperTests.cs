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
        public void FullMapping(string uri, string expected) {
            var mapped = new UrlToPathMapper().GetPath(new Uri(uri));
            Assert.AreEqual(expected, mapped);
        }

        [Test]
        [Row("http://x.com/a?k=p",  "something.zip", @"x.com\something.zip")]
        [Row("http://x.com/a",      "something.zip", @"x.com\something.zip")]
        [Row("http://x.com/a/",     "something.zip", @"x.com\a\something.zip")]
        [Row("http://x.com/a/b",    "something.zip", @"x.com\a\something.zip")]
        public void FullMappingWithFileName(string uri, string fileName, string expected) {
            var mapped = new UrlToPathMapper().GetPath(new Uri(uri), fileName);
            Assert.AreEqual(expected, mapped);
        }
    }
}
