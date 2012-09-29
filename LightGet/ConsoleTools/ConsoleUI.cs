using System;
using System.Collections.Generic;
using System.Linq;

namespace LightGet.ConsoleTools {
    public static class ConsoleUI {
        #region Disposer Class
        private class Disposer : IDisposable {
            private readonly Action dispose;

            public Disposer(Action dispose) {
                this.dispose = dispose;
            }

            public void Dispose() {
                this.dispose();
            }
        }
        #endregion

        public static IDisposable ForegroundColor(ConsoleColor color) {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            return new Disposer(() => Console.ForegroundColor = oldColor);
        }

        public static void WriteLine(ConsoleColor color, string format, params object[] args) {
            using (ConsoleUI.ForegroundColor(color))
                Console.WriteLine(format, args);
        }

        public static void WriteLine(ConsoleColor color, object value) {
            using (ConsoleUI.ForegroundColor(color))
                Console.WriteLine(value);
        }
    }
}
