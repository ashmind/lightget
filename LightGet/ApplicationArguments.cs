using System;
using LightGet.ConsoleTools;

namespace LightGet {
    public class ApplicationArguments {
        public bool Recursive   { get; set; }

        public string User      { get; set; }
        public string Password  { get; set; }

        public bool IgnoreCertificateValidation { get; set; }

        [Positional(0)]
        public Uri Url          { get; set; }
    }
}