using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace LightGet {
    [Serializable]
    public class DownloadException : Exception {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DownloadException() {
        }

        public DownloadException(string message) : base(message) {
        }

        public DownloadException(string message, Exception inner) : base(message, inner) {
        }

        protected DownloadException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }
    }
}
