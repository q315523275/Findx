using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Findx.Configuration
{
    internal sealed class JsonConfigurationFileParser
    {
        private readonly SortedDictionary<string, string> _data =
            new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _paths = new Stack<string>();

        private JsonConfigurationFileParser()
        {
        }

        public static IDictionary<string, string> Parse(string input)
        {
            return new JsonConfigurationFileParser().ParseString(input);
        }

        private IDictionary<string, string> ParseString(string json)
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            using (var doc = JsonDocument.Parse(json, jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    throw new FormatException($"{doc.RootElement.ValueKind}顶层JSON元素无效");
                VisitObjectElement(doc.RootElement);
            }

            return _data;
        }

        private void VisitObjectElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (var property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            SetNullIfElementIsEmpty(isEmpty);
        }

        private void VisitArrayElement(JsonElement element)
        {
            var index = 0;

            foreach (var arrayElement in element.EnumerateArray())
            {
                EnterContext(index.ToString());
                VisitValue(arrayElement);
                ExitContext();
                index++;
            }

            SetNullIfElementIsEmpty(index == 0);
        }

        private void SetNullIfElementIsEmpty(bool isEmpty)
        {
            if (isEmpty && _paths.Count > 0) _data[_paths.Peek()] = null;
        }

        private void VisitValue(JsonElement value)
        {
            Debug.Assert(_paths.Count > 0);

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitObjectElement(value);
                    break;

                case JsonValueKind.Array:
                    VisitArrayElement(value);
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    var key = _paths.Peek();
                    if (_data.ContainsKey(key)) throw new FormatException($"{key}重复");
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException($"{value.ValueKind}不支持的JSON令牌");
            }
        }

        private void EnterContext(string context)
        {
            _paths.Push(_paths.Count > 0 ? _paths.Peek() + ConfigurationPath.KeyDelimiter + context : context);
        }

        private void ExitContext()
        {
            _paths.Pop();
        }
    }
}