using System;
using System.Collections.Generic;
using System.Linq;

namespace LightGet.ConsoleTools {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PositionalAttribute : Attribute {
        public int Position { get; private set; }

        public PositionalAttribute(int position) {
            Position = position;
        }
    }
}
