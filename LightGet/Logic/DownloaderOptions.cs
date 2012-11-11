using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LightGet.Logic {
    public class DownloaderOptions {
        public ICredentials Credentials { get; set; }
        public Action<DownloaderProgress> ReportProgress { get; set; }

        public DownloaderOptions() {
            this.ReportProgress = delegate { };
        }
    }
}
