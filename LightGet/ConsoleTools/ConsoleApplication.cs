using System;
using System.Collections.Generic;
using System.Linq;

namespace LightGet.ConsoleTools {
    public class ConsoleApplication {
        public void Run(Action action) {
            action();
        }
    }
}
