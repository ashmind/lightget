using System;
using System.Collections.Generic;
using System.Linq;
using LightGet.ConsoleTools;

namespace LightGet.Logic {
    public class ConsoleLogger : ILogger {
        public string Prefix { get; set; }

        public virtual void LogDebug(string format, params object[] args) {
            ConsoleUI.WriteLine(ConsoleColor.Gray, Prefix + format, args);
        }

        public virtual void LogMessage(string format, params object[] args) {
            Console.WriteLine(Prefix + format, args);
        }

        public virtual void LogWarning(string format, params object[] args) {
            ConsoleUI.WriteLine(ConsoleColor.Yellow, Prefix + format, args);
        }

        public virtual void LogError(string format, params object[] args) {
            ConsoleUI.WriteLine(ConsoleColor.Red, Prefix + format, args);
        }
    }
}
