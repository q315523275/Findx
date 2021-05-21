using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace Findx.Configuration
{
    internal sealed class JsonConfigurationFileParser
    {
        private JsonConfigurationFileParser() { }

        private readonly SortedDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _paths = new Stack<string>();

        public static IDictionary<string, string> Parse(string input)
            => new JsonConfigurationFileParser().ParseStream(input);

        private IDictionary<string, string> ParseStream(string json)
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            using (JsonDocument doc = JsonDocument.Parse(json, jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException($"{doc.RootElement.ValueKind}顶层JSON元素无效");
                }
                VisitElement(doc.RootElement);
            }

            return _data;
        }

        private void VisitElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            if (isEmpty && _paths.Count > 0)
            {
                _data[_paths.Peek()] = null;
            }
        }

        private void VisitValue(JsonElement value)
        {
            Debug.Assert(_paths.Count > 0);

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitElement(value);
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (JsonElement arrayElement in value.EnumerateArray())
                    {
                        EnterContext(index.ToString());
                        VisitValue(arrayElement);
                        ExitContext();
                        index++;
                    }
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string key = _paths.Peek();
                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException($"{key}重复");
                    }
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException($"{value.ValueKind}不支持的JSON令牌");
            }
        }

        private void EnterContext(string context) =>
            _paths.Push(_paths.Count > 0 ?
                _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _paths.Pop();
    }
}
