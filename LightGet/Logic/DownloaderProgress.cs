using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightGet.Logic {
    public struct DownloaderProgress {
        public FileInfo File { get; set; }

        public long BytesTotal { get; set; }
        public long BytesDownloadedBefore { get; set; }

        public long BytesDownloaded { get; set; }
        public TimeSpan TimeElapsed { get; set; }

        public long BytesRemaining {
            get { return BytesTotal - BytesDownloaded - BytesDownloadedBefore; }
        }
    }
}
