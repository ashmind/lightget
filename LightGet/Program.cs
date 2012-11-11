using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
            var lastPercent = -1.0;
            var lastTimeReported = (string)null;
            
            var result = downloader.Download(url, new DownloaderOptions {
                Credentials = credentials,
                ReportProgress = p => ReportDownloadProgress(p, ref lastPercent, ref lastTimeReported)
            });
            SetTaskBarProgress(TaskbarProgressBarState.NoProgress);

            return result;
        }

        private static void ReportDownloadProgress(DownloaderProgress progress, ref double lastPercent, ref string lastTimeReported) {
            if (progress.BytesTotal <= 0) { // length is unknown
                SetTaskBarProgress(TaskbarProgressBarState.Indeterminate);
                return;
            }

            var percent = 100 * (double)(progress.BytesDownloadedBefore + progress.BytesDownloaded) / progress.BytesTotal;
            if (Math.Abs(percent - lastPercent) >= 0.1) {
                Console.SetCursorPosition(ConsoleIndent.Length, Console.CursorTop);
                Console.Write("{0:F1} %", percent);
                lastPercent = percent;

                SetTaskBarProgress(TaskbarProgressBarState.Normal);
                SetTaskBarProgress(percent);
                Console.Title = percent.ToString("F1") + "%";
            }
            
            var msRemaining = progress.BytesRemaining * (progress.TimeElapsed.TotalMilliseconds / progress.BytesDownloaded);
            var timeRemaining = TimeSpan.FromMilliseconds(msRemaining);
            var formatted = FormatRemainingTime(timeRemaining);

            if (formatted == lastTimeReported)
                return;

            Console.SetCursorPosition(ConsoleIndent.Length + "100.0 % ".Length, Console.CursorTop);
            Console.Write("Remaining: {0}.", formatted);
            lastTimeReported = formatted;
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
