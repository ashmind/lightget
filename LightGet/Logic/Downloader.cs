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
        private readonly Func<Uri, string, FileInfo> mapPath;

        public Downloader(Func<Uri, string, FileInfo> mapPath) {
            this.mapPath = mapPath;
        }

        public DownloaderResult Download(Uri url, DownloaderOptions options) {
            var head = CreateRequest(url, options);
            head.Method = "HEAD";
            var headResponse = (HttpWebResponse)head.GetResponse();
            headResponse.Close();

            if (this.IsRedirect(headResponse)) {
                var target = headResponse.Headers[HttpResponseHeader.Location];
                if (target.IsNullOrEmpty())
                    throw new Exception("Response is a redirect, but location header was not set.");

                return Download(new Uri(target), options);
            }

            if (headResponse.StatusCode != HttpStatusCode.OK) {
                throw new NotSupportedException(string.Format("HEAD request returned a response code {0}: {1} that is currently not supported.", headResponse.StatusCode, headResponse.StatusDescription));
            }

            while (true) {
                try {
                    return Download(url, headResponse, options);
                }
                catch (DownloadException ex) {
                    options.ReportError("Download error {0}, retrying.", ex.Message);
                }
            }
        }

        private DownloaderResult Download(Uri url, HttpWebResponse headResponse, DownloaderOptions options) {
            var fullLength = headResponse.ContentLength;
            var fileName = GetFileName(headResponse);
            var file = this.mapPath(url, fileName);
            file.Directory.Create();

            options.ReportMessage("-> {0}.", file.FullName);

            var get = CreateRequest(url, options);
            get.Method = "GET";
            var lengthDownloadedBefore = 0L;
            var fileMode = FileMode.Append;
            if (file.Exists && file.Length > 0) {
                if (file.Length == fullLength) {
                    options.ReportMessage("File is already fully downloaded.");
                    return new DownloaderResult(url, file, headResponse.ContentType);
                }

                if (file.Length < fullLength) {
                    options.ReportMessage("File already exists, requesting range {0}-{1}.", file.Length, headResponse.ContentLength);
                    get.AddRange(file.Length, fullLength);
                    lengthDownloadedBefore = file.Length;
                }
                else if (fullLength <= 0) {
                    options.ReportMessage("File already exists, but server did not provide length, so it will be fully redownloaded.");
                    fileMode = FileMode.OpenOrCreate;
                }
                else {
                    options.ReportMessage("File already exists, but is larger than file on server, so it will be fully redownloaded. ");
                    fileMode = FileMode.OpenOrCreate;
                }
            }

            var response = get.GetResponse();
            var downloadedTotal = 0L;
            using (response)
            using (var fileStream = file.Open(fileMode, FileAccess.Write, FileShare.Read))
            using (var webStream = response.GetResponseStream()) {
                var buffer = new byte[1024];
                var time = new Stopwatch();
                time.Start();

                while (true) {
                    int count;
                    try {
                        count = webStream.Read(buffer, 0, buffer.Length);
                    }
                    catch (IOException ex) {
                        throw new DownloadException(ex.Message, ex);
                    }
                    if (count == 0)
                        break;

                    downloadedTotal += count;
                    fileStream.Write(buffer, 0, count);

                    options.ReportProgress(new DownloaderProgress {
                        BytesDownloadedBefore = lengthDownloadedBefore,
                        BytesDownloaded = downloadedTotal,
                        BytesTotal = fullLength,
                        TimeElapsed = time.Elapsed
                    });
                }
            }

            options.ReportMessage("Download completed.");
            var contentType = new ContentType(headResponse.ContentType);
            return new DownloaderResult(url, file, contentType.MediaType);
        }

        private static string GetFileName(HttpWebResponse response) {
            var contentDisposition = response.Headers["Content-Disposition"];
            if (contentDisposition.IsNullOrEmpty())
                return null;

            var disposition = new ContentDisposition(contentDisposition);
            if (disposition.FileName.IsNullOrEmpty())
                return null;

            return disposition.FileName;
        }

        private static HttpWebRequest CreateRequest(Uri url, DownloaderOptions options) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Credentials = options.Credentials;
            request.AllowAutoRedirect = false;

            return request;
        }

        private bool IsRedirect(HttpWebResponse response) {
            return response.StatusCode == HttpStatusCode.MovedPermanently
                || response.StatusCode == HttpStatusCode.Redirect
                || response.StatusCode == HttpStatusCode.TemporaryRedirect;
        }
    }
}
