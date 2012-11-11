using System;
using System.Collections.Generic;
using System.Linq;
using LightGet.Logic;
using MbUnit.Framework;

namespace LightGet.Tests.Of.Logic {
    [TestFixture]
    public class PathMappingRuleConverterTests {
        [Test]
        [Factory("GetStringRulePairs")]
        public void ConvertFromInvariantString(string value, PathMappingRule expectedRule) {
            var result = (PathMappingRule)new PathMappingRuleConverter().ConvertFromInvariantString(value);
            var properties = typeof(PathMappingRule).GetProperties();

            Assert.AreElementsEqualIgnoringOrder(
                properties.Select(p => new { p.Name, Value = p.GetValue(expectedRule) }),
                properties.Select(p => new { p.Name, Value = p.GetValue(result) })
            );
        }

        public IEnumerable<object[]> GetStringRulePairs() {
            yield return new object[] { "full", new PathMappingRule {IncludeHost = true, IncludePort = true, IncludeParentPath = true, IncludePath = true}};
            yield return new object[] { "host,port", new PathMappingRule { IncludeHost = true, IncludePort = true, IncludePath = false, IncludeParentPath = false } };
            yield return new object[] { "root", new PathMappingRule { IncludePath = true, IncludeParentPath = true, IncludeHost = false, IncludePort = false } };
            yield return new object[] { "relative", new PathMappingRule { IncludePath = true, IncludeParentPath = false, IncludeHost = false, IncludePort = false } };
        }
    }
}