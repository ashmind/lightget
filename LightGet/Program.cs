﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using LightGet.ConsoleTools;
using LightGet.Logic;
using Microsoft.WindowsAPICodePack.Taskbar;

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

            Console.CursorVisible = false;

            var mapper = new UrlToPathMapper();
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            var downloader = new Downloader((url, fileName) => new FileInfo(Path.Combine(directory.FullName, mapper.GetPath(url, fileName))));
            var extractor = new LinkExtractor();

            var credentials = arguments.User != null ? new NetworkCredential(arguments.User, arguments.Password) : null;
            var visited = new HashSet<Uri>();

            var queue = new Queue<Uri>();
            queue.Enqueue(arguments.Url);

            while (queue.Count > 0) {
                var url = queue.Dequeue();
                var result = Download(downloader, url, credentials);

                visited.Add(url);
                if (!arguments.Recursive)
                    break;

                using (var reader = result.File.OpenText()) {
                    var links = extractor.ExtractLinks(result.Url, reader, result.ContentType);
                    foreach (var link in links.Where(l => !visited.Contains(l))) {
                        queue.Enqueue(link);
                    }
                }
            }

            Console.CursorVisible = true;
        }

        private static DownloaderResult Download(Downloader downloader, Uri url, ICredentials credentials) {
            var lastPercent = 0.0;
            var lastTimeReported = (string)null;

            ConsoleUI.WriteLine(ConsoleColor.White, url);
            var result = downloader.Download(url, new DownloaderOptions {
                Credentials = credentials,
                ReportMessage = (format, args) => Console.WriteLine(format, args),
                ReportError = (format, args) => {
                    SetTaskBarProgress(TaskbarProgressBarState.Error);
                    ConsoleUI.WriteLine(ConsoleColor.Red, format, args);
                },
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
                Console.SetCursorPosition(0, Console.CursorTop);
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

            Console.SetCursorPosition("100.0 % ".Length, Console.CursorTop);
            Console.Write("Remaining: {0}.", formatted);
            lastTimeReported = formatted;
        }

        private static void SetTaskBarProgress(double percent) {
            if (!TaskbarManager.IsPlatformSupported)
                return;

            TaskbarManager.Instance.SetProgressValue((int)percent, 100);
        }

        private static void SetTaskBarProgress(TaskbarProgressBarState state) {
            if (!TaskbarManager.IsPlatformSupported)
                return;

            TaskbarManager.Instance.SetProgressState(state);
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
