using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using AshMind.Extensions;

namespace LightGet.Logic {
    public class Downloader {
        public DownloaderResult Download(DownloaderArguments arguments) {
            var head = CreateRequest(arguments);
            head.Method = "HEAD";
            var headResponse = (HttpWebResponse)head.GetResponse();

            while (true) {
                try {
                    return Download(arguments, headResponse);
                }
                catch (DownloadException ex) {
                    arguments.ReportError("Download error {0}, retrying.", ex.Message);
                }
            }
        }

        private DownloaderResult Download(DownloaderArguments arguments, HttpWebResponse headResponse) {
            var fullLength = headResponse.ContentLength;
            var fileName = GetFileName(headResponse);
            arguments.ReportMessage("File will be saved to {0}.", fileName);

            var get = CreateRequest(arguments);
            get.Method = "GET";
            var file = new FileInfo(fileName);
            var lengthDownloadedBefore = 0L;
            if (file.Exists && file.Length > 0) {
                if (file.Length == headResponse.ContentLength) {
                    arguments.ReportMessage("File is already fully downloaded.");
                    return new DownloaderResult(file);
                }

                arguments.ReportMessage("File already exists, requesting range {0}-{1}.", file.Length, headResponse.ContentLength);
                get.AddRange(file.Length, fullLength);
                lengthDownloadedBefore = file.Length;
            }

            var response = get.GetResponse();
            var downloadedTotal = 0L;
            using (var fileStream = file.Open(FileMode.Append, FileAccess.Write, FileShare.Read))
            using (var webStream = response.GetResponseStream()) {
                var buffer = new byte[1024];
                var time = new Stopwatch();
                time.Start();

                while (downloadedTotal < response.ContentLength) {
                    int count;
                    try {
                        count = webStream.Read(buffer, 0, (int)Math.Min(response.ContentLength - downloadedTotal, buffer.Length));
                    }
                    catch (IOException ex) {
                        throw new DownloadException(ex.Message, ex);
                    }
                    downloadedTotal += count;
                    fileStream.Write(buffer, 0, count);

                    arguments.ReportProgress(new DownloaderProgress {
                        BytesDownloadedBefore = lengthDownloadedBefore,
                        BytesDownloaded = downloadedTotal,
                        BytesTotal = fullLength,
                        TimeElapsed = time.Elapsed
                    });
                }
            }

            arguments.ReportMessage("Download completed.");
            return new DownloaderResult(file);
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

        private static HttpWebRequest CreateRequest(DownloaderArguments arguments) {
            var request = (HttpWebRequest)WebRequest.Create(arguments.Url);
            request.Credentials = arguments.Credentials;

            return request;
        }
    }
}
