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
            var result = new CommandLineParser().Parse<PrimitiveClass>(new[] { arg });
            Assert.AreEqual(expectedValue, getValue(result));
        }

        [Test]
        public void Positional() {
            var result = new CommandLineParser().Parse<ClassWithPositionalProperties>(new[] { "First", "Second" });

            Assert.AreEqual("First", result.First);
            Assert.AreEqual("Second", result.Second);
        }

        [Test]
        public void Enums() {
            var result = new CommandLineParser().Parse<ClassWithEnum>(new[] { "--enum-value=long-name-value" });
            Assert.AreEqual(ClassWithEnum.SomeEnum.LongNameValue, result.EnumValue);
        }
    }
}
