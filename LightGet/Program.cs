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
                Main(arguments);
            });
        }

        private static void Main(Arguments arguments) {
            if (arguments.IgnoreCertificateValidation)
                ServicePointManager.ServerCertificateValidationCallback = (x1, x2, x3, x4) => true;

            ConsoleUI.WriteLine(ConsoleColor.White, arguments.Url);
            var head = CreateRequest(arguments);
            head.Method = "HEAD";
            var headResponse = (HttpWebResponse)head.GetResponse();
            var fullLength = headResponse.ContentLength;
            var fileName = GetFileName(headResponse);
            Console.WriteLine("File will be saved to {0}.", fileName);

            var get = CreateRequest(arguments);
            get.Method = "GET";
            var file = new FileInfo(fileName);
            var lengthDownloadedBefore = 0L;
            if (file.Exists && file.Length > 0) {
                Console.WriteLine("File already exist, requesting range {0}-{1}.", file.Length, headResponse.ContentLength);
                get.AddRange(file.Length, fullLength - file.Length);
                lengthDownloadedBefore = file.Length;
            }

            var response = get.GetResponse();
            var downloadedTotal = 0L;
            using (var fileStream = file.Open(FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var webStream = response.GetResponseStream()) {
                var buffer = new byte[1024];
                var time = new Stopwatch();
                time.Start();

                var lastTimeReported = (string)null;
                var lastPercent = -1.0;

                while (downloadedTotal < response.ContentLength) {
                    var count = webStream.Read(buffer, 0, (int)Math.Min(response.ContentLength - downloadedTotal, buffer.Length));
                    downloadedTotal += count;

                    var percent = 100 * (double)(lengthDownloadedBefore + downloadedTotal) / fullLength;
                    if (Math.Abs(percent - lastPercent) >= 0.1) {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write("{0:F1} %", percent);
                        lastPercent = percent;
                    }

                    fileStream.Write(buffer, 0, count);

                    var msRemaining = (response.ContentLength - downloadedTotal) * (time.Elapsed.TotalMilliseconds / downloadedTotal);
                    ReportRemainingTime("100.0 % ".Length, msRemaining, ref lastTimeReported);
                }
            }

            Console.WriteLine("Download completed.");
        }

        private static void ReportRemainingTime(int position, double msRemaining, ref string lastReported) {
            var timeRemaining = TimeSpan.FromMilliseconds(msRemaining);
            var formatted = FormatRemainingTime(timeRemaining);

            if (formatted == lastReported)
                return;

            Console.SetCursorPosition(position, Console.CursorTop);
            Console.Write("Remaining: {0}.", formatted);
            lastReported = formatted;
        }

        private static string FormatRemainingTime(TimeSpan timeRemaining) {
            if (timeRemaining.TotalDays >= 1)
                return timeRemaining.Days + " days " + timeRemaining.Hours + " hours";

            if (timeRemaining.TotalHours >= 1)
                return timeRemaining.Hours + " hours " + timeRemaining.Minutes + " minutes";

            if (timeRemaining.TotalMinutes >= 1)
                return timeRemaining.Minutes + " minutes" + timeRemaining.Seconds + " seconds";

            return timeRemaining.Seconds + " seconds";
        }

        private static string GetFileName(HttpWebResponse response) {
            var fileNameFromUri = response.ResponseUri.LocalPath.SubstringAfterLast("/");

            var contentDisposition = response.Headers["Content-Disposition"];
            if (contentDisposition.IsNullOrEmpty())
                return fileNameFromUri;

            var disposition = new ContentDisposition(contentDisposition);
            if (disposition.FileName.IsNullOrEmpty())
                return fileNameFromUri;

            return disposition.FileName;
        }

        private static HttpWebRequest CreateRequest(Arguments arguments) {
            var request = (HttpWebRequest)WebRequest.Create(arguments.Url);
            if (arguments.User != null)
                request.Credentials = new NetworkCredential(arguments.User, arguments.Password);
            return request;
        }
    }
}
