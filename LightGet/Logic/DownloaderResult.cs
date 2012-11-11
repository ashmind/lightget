using System;
using System.IO;

namespace LightGet.Logic {
    public class DownloaderResult {
        public FileInfo File { get; private set; }
        public string ContentType { get; private set; }
        public Uri Url { get; private set; }

        public DownloaderResult(Uri url, FileInfo file, string contentType) {
            this.Url = url;
            this.File = file;
            this.ContentType = contentType;
        }
    }
}