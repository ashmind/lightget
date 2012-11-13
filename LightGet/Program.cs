using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LightGet.ConsoleTools;
using LightGet.Logic;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace LightGet {
    public class Program {
        #region GetConsoleWindow

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        #endregion

        #region LoggerForDownloader Class

        private class LoggerForDownloader : ConsoleLogger {
            public override void LogError(string format, params object[] args) {
                SetTaskBarProgress(TaskbarProgressBarState.Error);
                base.LogError(format, args);
            }
        }

        #endregion

        #region ProgressState Class

        private class ProgresState {
            public double LastPercent             { get; set; }
            public string LastRemainingTimeString { get; set; }
            public string LastBytesPerSecondString      { get; set; }
        }

        #endregion

        private const string ConsoleIndent = "  ";
        private static IntPtr consoleWindowHandle;

        public static void Main(string[] args) {
            new ConsoleApplication().Run(() => {
                var arguments = new CommandLineParser().Parse<ApplicationArguments>(args);
                Main(arguments);
            });
        }

        private static void Main(ApplicationArguments arguments) {
            EnsureConsoleWindowHandle();
            if (arguments.IgnoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = (x1, x2, x3, x4) => true;

            Console.Title = "LightGet";
            Console.CursorVisible = false;

            var loggerForDownloader = new LoggerForDownloader { Prefix = ConsoleIndent };

            var mapper = new UrlToPathMapper(arguments.OutputPathFormat, arguments.Url);
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            var downloader = new Downloader(loggerForDownloader, (url, fileName) => new FileInfo(Path.Combine(directory.FullName, mapper.GetPath(url, fileName))));
            var extractor = new LinkExtractor();

            var credentials = arguments.User != null ? new NetworkCredential(arguments.User, arguments.Password) : null;
            var visited = new HashSet<Uri>();

            var queue = new Queue<Uri>();
            queue.Enqueue(arguments.Url);

            while (queue.Count > 0) {
                var url = queue.Dequeue();
                ConsoleUI.WriteLine(ConsoleColor.White, url);

                DownloaderResult result;
                try {
                    result = Download(downloader, url, credentials);
                }
                catch (Exception ex) {
                    ConsoleUI.WriteLine(ConsoleColor.Red, ex.Message);
                    continue;
                }
                finally {
                    Console.WriteLine();
                }

                visited.Add(url);
                if (arguments.FollowLinks == LinkFollowingRule.None)
                    break;

                using (var reader = result.File.OpenText()) {
                    var links = extractor.ExtractLinks(result.Url, reader, result.ContentType);
                    foreach (var link in links.Where(l => !visited.Contains(l))) {
                        if (!ShouldFollow(link, arguments))
                            continue;

                        queue.Enqueue(link);
                    }
                }
            }

            Console.CursorVisible = true;
        }

        private static bool ShouldFollow(Uri link, ApplicationArguments arguments) {
            if (arguments.FollowLinks == LinkFollowingRule.All)
                return true;

            if (arguments.FollowLinks == LinkFollowingRule.SamePath)
                return link.ToString().Contains(arguments.Url.ToString());

            throw new NotSupportedException(string.Format("FollowLinks rule {0} is not supported", arguments.FollowLinks));
        }

        private static DownloaderResult Download(Downloader downloader, Uri url, ICredentials credentials) {
            var progressState = new ProgresState {
                LastPercent = -1.0,
                LastRemainingTimeString = null,
                LastBytesPerSecondString = null
            };
            
            var result = downloader.Download(url, new DownloaderOptions {
                Credentials = credentials,
                ReportProgress = p => ReportDownloadProgress(p, progressState)
            });
            SetTaskBarProgress(TaskbarProgressBarState.NoProgress);

            return result;
        }

        private static void ReportDownloadProgress(DownloaderProgress progress, ProgresState progressState) {
            if (progress.BytesTotal <= 0) { // length is unknown
                SetTaskBarProgress(TaskbarProgressBarState.Indeterminate);
                return;
            }

            var indent = ConsoleIndent.Length;
            var percent = 100 * (double)(progress.BytesDownloadedBefore + progress.BytesDownloaded) / progress.BytesTotal;
            if (Math.Abs(percent - progressState.LastPercent) >= 0.1) {
                Console.SetCursorPosition(indent, Console.CursorTop);
                Console.Write("{0,4:F1}% |", percent);
                progressState.LastPercent = percent;

                SetTaskBarProgress(TaskbarProgressBarState.Normal);
                SetTaskBarProgress(percent);
            }

            indent += "99.9% | ".Length;
            var bps = progress.BytesDownloaded/progress.TimeElapsed.TotalSeconds;
            var bpsString = FormatBytesPerSecond(bps);
            if (bpsString != progressState.LastBytesPerSecondString) {
                Console.SetCursorPosition(indent, Console.CursorTop);
                Console.Write(bpsString.PadRight("999 TB/s".Length));
                Console.Write(" |");
                progressState.LastBytesPerSecondString = bpsString;
            }

            indent += "999 TB/s | ".Length;
            var msRemaining = progress.BytesRemaining * (progress.TimeElapsed.TotalMilliseconds / progress.BytesDownloaded);
            var timeRemaining = TimeSpan.FromMilliseconds(msRemaining);
            var timeRemainingString = FormatRemainingTime(timeRemaining);
            if (timeRemainingString != progressState.LastRemainingTimeString) {
                Console.SetCursorPosition(indent, Console.CursorTop);
                Console.Write("Remaining: {0}.".PadRight(40), timeRemainingString);
                progressState.LastRemainingTimeString = timeRemainingString;
                Console.Title = string.Format("{0}: {1}", Regex.Replace(timeRemainingString, @"(\d)\s(\w)\w*", "$1$2"), progress.File.Name);
            }
        }

        private static void EnsureConsoleWindowHandle() {
            if (!TaskbarManager.IsPlatformSupported)
                return;

            consoleWindowHandle = GetConsoleWindow();
            if (consoleWindowHandle == IntPtr.Zero)
                ConsoleUI.WriteLine(ConsoleColor.Yellow, "Unable to get console window handle, progress will not be reported in taskbar.");
        }

        private static void SetTaskBarProgress(double percent) {
            if (!TaskbarManager.IsPlatformSupported || consoleWindowHandle == IntPtr.Zero)
                return;

            TaskbarManager.Instance.SetProgressValue((int)percent, 100, consoleWindowHandle);
        }

        private static void SetTaskBarProgress(TaskbarProgressBarState state) {
            if (!TaskbarManager.IsPlatformSupported || consoleWindowHandle == IntPtr.Zero)
                return;

            TaskbarManager.Instance.SetProgressState(state, consoleWindowHandle);
        }

        private static string FormatRemainingTime(TimeSpan timeRemaining) {
            Func<int, string> s = n => n > 1 ? "s" : "";

            if (timeRemaining.TotalDays >= 1)
                return string.Format("{0} day{1} {2} hour{3}", timeRemaining.Days, s(timeRemaining.Days), timeRemaining.Hours, s(timeRemaining.Hours));

            if (timeRemaining.TotalHours >= 1)
                return string.Format("{0} hour{1} {2} minute{3}", timeRemaining.Hours, s(timeRemaining.Hours), timeRemaining.Minutes, s(timeRemaining.Minutes));

            if (timeRemaining.TotalMinutes >= 1)
                return timeRemaining.Minutes + " minute" + s(timeRemaining.Minutes);

            return timeRemaining.Seconds + " second" + s(timeRemaining.Seconds);
        }

        private static readonly IList<Tuple<long, string>> ByteUnits = new List<Tuple<long, string>> {
            Tuple.Create((long)Math.Pow(10, 12), "T"),
            Tuple.Create((long)Math.Pow(10,  9), "G"),
            Tuple.Create((long)Math.Pow(10,  6), "M"),
            Tuple.Create(1000L, "K"),
            Tuple.Create(1L,    "")
        };
        
        private static string FormatBytesPerSecond(double bps) {
            foreach (var unit in ByteUnits) {
                var value = bps/unit.Item1;
                if (value > 1)
                    return string.Format("{0} {1}B/s", Math.Round(value, value > 10 ? 0 : 1), unit.Item2);
            }

            throw new Exception("This is never reached.");
        }
    }
}
