using System;
using System.Collections.Generic;
using System.Linq;
using LightGet.ConsoleTools;

namespace LightGet {
    public class Program {
        public static void Main(string[] args) {
            new ConsoleApplication().Run(() => {
                var arguments = new CommandLineParser().Parse<Arguments>(args);
                Main(arguments);
            });
        }

        private static void Main(Arguments arguments) {
            
        }
    }
}
