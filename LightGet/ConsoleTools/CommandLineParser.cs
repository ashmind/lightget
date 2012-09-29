using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AshMind.Extensions;

namespace LightGet.ConsoleTools {
    public class CommandLineParser {
        private class StructuredArguments {
            public IDictionary<string, string> Named { get; private set; }
            public IList<string> Positional { get; private set; }

            public StructuredArguments() {
                this.Named = new Dictionary<string, string>();
                this.Positional = new List<string>();
            }
        }

        public T Parse<T>(string[] args) 
            where T : new()
        {
            var arguments = Analyze(args);
            var result = new T();

            var properties = TypeDescriptor.GetProperties(typeof(T));
            foreach (var named in arguments.Named) {
                if (named.Key.Length == 1)
                    throw new NotImplementedException();

                var propertyName = ToPropertyName(named.Key);
                var property = properties[propertyName];
                if (property == null)
                    throw new FormatException(string.Format("Property {0} was not found on {1}.", propertyName, typeof(T)));

                var value = ConvertValue(property, named.Value);
                property.SetValue(result, value);
            }

            if (!arguments.Positional.Any())
                return result;

            var propertiesByPosition = (
                from PropertyDescriptor property in properties
                let attribute = (PositionalAttribute)property.Attributes[typeof(PositionalAttribute)]
                where attribute != null
                select new { position = attribute.Position, property }
            ).ToDictionary(x => x.position, x => x.property);
            for (var i = 0; i < arguments.Positional.Count; i++) {
                var property = propertiesByPosition[i];
                property.SetValue(result, ConvertValue(property, arguments.Positional[i]));
            }

            return result;
        }

        private static object ConvertValue(PropertyDescriptor property, string valueString) {
            if (valueString == null && property.PropertyType == typeof(bool))
                return true;
            
            return property.Converter.ConvertFromInvariantString(valueString);
        }

        private string ToPropertyName(string parameterName) {
            return Regex.Replace(parameterName, "(?:^|-).", match => match.Value.Last().ToUpperInvariant().ToString());
        }

        private StructuredArguments Analyze(IEnumerable<string> args) {
            var result = new StructuredArguments();
            var currentName = (string)null;
            foreach (var arg in args) {
                if (arg.StartsWith("--")) {
                    if (currentName != null) {
                        result.Named.Add(currentName, null);
                        currentName = null;
                    }

                    var parts = arg.TrimStart('-').Split(new[] {'='}, 2);
                    result.Named.Add(parts[0], parts.ElementAtOrDefault(1));

                    continue;
                }

                if (arg.StartsWith("-")) {
                    if (currentName != null)
                        result.Named.Add(currentName, null);

                    currentName = arg.TrimStart('-');
                    continue;
                }

                if (currentName != null) {
                    result.Named.Add(currentName, arg);
                    currentName = null;
                }
                else {
                    result.Positional.Add(arg);
                }
            }

            return result;
        }
    }
}
