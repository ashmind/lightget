using System;
using LightGet.ConsoleTools;
using LightGet.Logic;

namespace LightGet {
    public class ApplicationArguments {
        [Positional(0)]
        public Uri Url { get; set; }

        public LinkFollowingRule FollowLinks { get; set; }
        public PathMappingRule OutputPathFormat { get; set; }

        public string User      { get; set; }
        public string Password  { get; set; }

        public bool IgnoreCertificateValidation { get; set; }
    }
}