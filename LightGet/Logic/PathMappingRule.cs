using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LightGet.Logic {
    [TypeConverter(typeof(PathMappingRuleConverter))]
    public class PathMappingRule {
        public PathMappingRule() {
            this.IncludeHost = true;
            this.IncludePort = true;
            this.IncludePath = true;
            this.IncludeParentPath = true;
        }

        public bool IncludeHost        { get; set; }
        public bool IncludePort        { get; set; }
        public bool IncludeParentPath  { get; set; }
        public bool IncludePath        { get; set; }
    }
}
