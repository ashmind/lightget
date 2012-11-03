using System.IO;

namespace LightGet.Logic {
    public class DownloaderResult {
        public FileInfo File { get; set; }
        
        public DownloaderResult(FileInfo file) {
            this.File = file;
        }
    }
}