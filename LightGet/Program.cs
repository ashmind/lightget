using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using LightGet.ConsoleTools;
using LightGet.Logic;

namespace LightGet {
    public class Program {
        public static void Main(string[] args) {
            new ConsoleApplication().Run(() => {
                var arguments = new CommandLineParser().Parse<ApplicationArguments>(args);
                Main(arguments);
            });
        }

        private static void Main(ApplicationArguments arguments) {
            if (arguments.IgnoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = (x1, x2, x3, x4) => true;

            var downloader = new Logic.Downloader();
            var lastPercent = 0.0;
            var lastTimeReported = (string)null;

            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            ConsoleUI.WriteLine(ConsoleColor.White, arguments.Url);
            Console.CursorVisible = false;
            downloader.Download(new DownloaderArguments(arguments.Url, directory) {
                Credentials = arguments.User != null ? new NetworkCredential(arguments.User, arguments.Password) : null,

                ReportMessage = (format, args) => Console.WriteLine(format, args),
                ReportError = (format, args) => ConsoleUI.WriteLine(ConsoleColor.Red, format, args),
                ReportProgress = p => ReportDownloadProgress(p, ref lastPercent, ref lastTimeReported)
            });
            Console.CursorVisible = true;
        }

        private static void ReportDownloadProgress(DownloaderProgress progress, ref double lastPercent, ref string lastTimeReported) {
            var percent = 100 * (double)(progress.BytesDownloadedBefore + progress.BytesDownloaded) / progress.BytesTotal;
            if (Math.Abs(percent - lastPercent) >= 0.1) {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("{0:F1} %", percent);
                lastPercent = percent;
            }
            
            var msRemaining = progress.BytesRemaining * (progress.TimeElapsed.TotalMilliseconds / progress.BytesDownloaded);
            var timeRemaining = TimeSpan.FromMilliseconds(msRemaining);
            var formatted = FormatRemainingTime(timeRemaining);

            if (formatted == lastTimeReported)
                return;

            Console.SetCursorPosition("100.0 % ".Length, Console.CursorTop);
            Console.Write("Remaining: {0}.", formatted);
            lastTimeReported = formatted;
        }

        private static string FormatRemainingTime(TimeSpan timeRemaining) {
            if (timeRemaining.TotalDays >= 1)
                return timeRemaining.Days + " days " + timeRemaining.Hours + " hours";

            if (timeRemaining.TotalHours >= 1)
                return timeRemaining.Hours + " hours " + timeRemaining.Minutes + " minutes";

            if (timeRemaining.TotalMinutes >= 1)
                return timeRemaining.Minutes + " minutes";

            return timeRemaining.Seconds + " seconds";
        }
    }
}
