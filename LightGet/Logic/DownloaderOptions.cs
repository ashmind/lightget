using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LightGet.Logic {
    public class DownloaderOptions {
        public delegate void Report(string format, params object[] args);

        public ICredentials Credentials { get; set; }

        public Report ReportMessage { get; set; }
        public Report ReportError { get; set; }
        public Action<DownloaderProgress> ReportProgress { get; set; }

        public DownloaderOptions() {
            this.ReportMessage = delegate { };
            this.ReportError = delegate { };
            this.ReportProgress = delegate { };
        }
    }
}
