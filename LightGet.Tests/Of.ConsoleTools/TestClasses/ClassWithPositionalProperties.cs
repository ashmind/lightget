using System;
using System.Collections.Generic;
using System.Linq;
using LightGet.ConsoleTools;

namespace LightGet.Tests.Of.ConsoleTools.TestClasses {
    public class ClassWithPositionalProperties {
        [Positional(0)]
        public string First  { get; set; }

        [Positional(1)]
        public string Second { get; set; }
    }
}
