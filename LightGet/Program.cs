using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using AshMind.Extensions;
using LightGet.ConsoleTools;

namespace LightGet {
    public class Program {
        public static void Main(string[] args) {
            new ConsoleApplication().Run(() => {
                var arguments = new CommandLineParser().Parse<Arguments>(args);
                Downloader.Download(arguments);
            });
        }
    }
}
