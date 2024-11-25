#if NET9_0_OR_GREATER
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Findx.Common;
using Findx.Extensions;
using Findx.Reflection;
using Findx.Utilities;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Findx.Swagger.Transformers;

internal sealed class XmlDocumentationTransformer : IOpenApiSchemaTransformer
{
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private static FileInfo[] _documentXmlFiles = [];
    private static IEnumerable<string> _documentFileNames = new List<string>();
    private static readonly List<XPathNavigator> Navigator = [];
    private static readonly AtomicBoolean Initialization = new(false);
    
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        InitializationFile();
        
        if (schema.Description is null && GetMemberName( context.JsonTypeInfo, context.JsonPropertyInfo) is { Length: > 0 } memberName && GetDescription(memberName) is { Length: > 0 } description)
        {
            schema.Description = description;
        }

        return Task.CompletedTask;
    }

    private string? GetDescription(string memberName)
    {
        if (_descriptions.TryGetValue(memberName, out var description))
        {
            return description;
        }
        
        var nodes = Navigator.Select(x => x.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary"));

        if (nodes.Any())
        {
            description = nodes.FirstOrDefault(x => x != null && x.Value.IsNotNullOrWhiteSpace())?.Value.Trim();
        }

        _descriptions[memberName] = description;

        return description;
    }

    private string? GetMemberName(JsonTypeInfo typeInfo, JsonPropertyInfo? propertyInfo)
    {
        if (!_documentFileNames.Contains(typeInfo.Type.Assembly.GetName().Name) && !_documentFileNames.Contains(propertyInfo?.DeclaringType.Assembly.GetName().Name))
        {
            return null;
        }
        
        if (propertyInfo is not null)
        {
            var typeName = propertyInfo.DeclaringType.FullName;
            var memberName = propertyInfo.AttributeProvider is MemberInfo member ? member.Name : $"{char.ToUpperInvariant(propertyInfo.Name[0])}{propertyInfo.Name[1..]}";
            var memberType = propertyInfo.AttributeProvider is PropertyInfo ? "P" : "F";

            return $"{memberType}:{typeName}{Type.Delimiter}{memberName}";
        }

        return $"T:{typeInfo.Type.FullName}";
    }

    private void InitializationFile()
    {
        if (Initialization.Value) return;
        
        _documentXmlFiles = DirectoryUtility.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
        _documentFileNames = _documentXmlFiles.Select(x => x.Name.RemovePostFix(".xml"));
        
        foreach (var xmlFileInfo in _documentXmlFiles)
        {
            using var reader = XmlReader.Create(xmlFileInfo.FullName);
            Navigator.Add(new XPathDocument(reader).CreateNavigator());
        }
        
        Initialization.CompareAndSet(false, true);
    }
}
#endif