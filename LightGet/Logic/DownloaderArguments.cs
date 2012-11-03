using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace LightGet.Logic {
    public class DownloaderArguments {
        public delegate void Report(string format, params object[] args);

        public Uri Url { get; private set; }
        public ICredentials Credentials { get; set; }

        public DirectoryInfo TargetDirectory { get; set; }

        public Report ReportMessage { get; set; }
        public Report ReportError { get; set; }
        public Action<DownloaderProgress> ReportProgress { get; set; }

        public DownloaderArguments(Uri url, DirectoryInfo targetDirectory) {
            this.Url = url;
            this.TargetDirectory = targetDirectory;
            this.ReportMessage = delegate { };
            this.ReportError = delegate { };
            this.ReportProgress = delegate { };
        }
    }
}
