using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AshMind.Extensions;

namespace LightGet.Logic {
    public class PathMappingRuleConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            var @string = value as string;
            if (@string != null)
                return Parse(@string);
            
            return base.ConvertFrom(context, culture, value);
        }

        private PathMappingRule Parse(string value) {
            var rule = new PathMappingRule();
            if (string.IsNullOrWhiteSpace(value))
                return rule;

            var parts = value.Split(',').ToSet(StringComparer.InvariantCultureIgnoreCase);
            var full = parts.Remove("full");

            rule.IncludeHost = parts.Remove("host") || full;
            rule.IncludePort = parts.Remove("port") || full;
            rule.IncludeParentPath = parts.Remove("root") || full;
            rule.IncludePath = parts.Remove("relative") || rule.IncludeParentPath;

            if (parts.Count > 0)
                throw new FormatException("Unknown rules: " + string.Join(",", parts) + ".");

            return rule;
        }
    }
}
