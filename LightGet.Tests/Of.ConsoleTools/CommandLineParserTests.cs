using System;
using System.Collections.Generic;
using LightGet.ConsoleTools;
using LightGet.Tests.Of.ConsoleTools.TestClasses;
using MbUnit.Framework;

namespace LightGet.Tests.Of.ConsoleTools {
    [TestFixture]
    public class CommandLineParserTests {
        [DynamicTestFactory]
        public IEnumerable<Test> Primitives() {
            yield return new TestCase("Primitive<Byte>", () => Primitive("--byte-value=1", c => c.ByteValue, 1));
            yield return new TestCase("Primitive<Int32>", () => Primitive("--int32-value=1", c => c.Int32Value, 1));
            yield return new TestCase("Primitive<Int64>", () => Primitive("--int64-value=1", c => c.Int64Value, 1));

            yield return new TestCase("Primitive<String>", () => Primitive("--string-value=xyz", c => c.StringValue, "xyz"));
        }

        private void Primitive<T>(string arg, Func<PrimitiveClass, T> getValue, T expectedValue) {
            var parser = new CommandLineParser();
            var result = parser.Parse<PrimitiveClass>(new[] { arg });

            Assert.AreEqual(expectedValue, getValue(result));
        }
    }
}
