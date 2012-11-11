using System;
using System.Collections.Generic;
using System.Linq;

namespace LightGet.Tests.Of.ConsoleTools.TestClasses {
    public class ClassWithEnum {
        public enum SomeEnum {
            Value,
            LongNameValue
        }

        public SomeEnum EnumValue { get; set; }
    }
}
