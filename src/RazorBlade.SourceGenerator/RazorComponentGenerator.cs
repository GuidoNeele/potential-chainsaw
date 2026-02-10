using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.SourceGenerator;

[Generator]
public class RazorComponentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        var razorFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".razor") && !file.Path.Contains("_Imports.razor"))
            .Select((file, cancellationToken) =>
            {
                var text = file.GetText(cancellationToken);
                return (Path: file.Path, Content: text?.ToString() ?? string.Empty);
            })
            .Collect();

        context.RegisterSourceOutput(razorFiles, (spc, files) =>
        {
            if (files.Length > 0)
            {
                GenerateHelpers(spc, files);
            }
        });
    }

    private void GenerateHelpers(SourceProductionContext context, System.Collections.Immutable.ImmutableArray<(string Path, string Content)> files)
    {
        var components = new List<ComponentInfo>();

        foreach (var file in files)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(file.Path);
            var codeBlock = ExtractCodeBlock(file.Content);
            var parameters = ExtractParameters(codeBlock);
            var namespaceName = ExtractNamespace(file.Path);

            components.Add(new ComponentInfo
            {
                Name = fileName,
                Namespace = namespaceName,
                Parameters = parameters
            });
        }

        // Generate extension methods for each component
        foreach (var component in components)
        {
            var source = GenerateExtensionMethods(component);
            context.AddSource($"{component.Name}Extensions.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private string ExtractCodeBlock(string content)
    {
        var codeStart = content.IndexOf("@code");
        if (codeStart < 0)
            return string.Empty;

        var braceStart = content.IndexOf('{', codeStart);
        if (braceStart < 0)
            return string.Empty;

        var braceCount = 1;
        var i = braceStart + 1;
        while (i < content.Length && braceCount > 0)
        {
            if (content[i] == '{') braceCount++;
            if (content[i] == '}') braceCount--;
            i++;
        }

        if (braceCount == 0)
        {
            return content.Substring(braceStart + 1, i - braceStart - 2);
        }

        return string.Empty;
    }

    private List<ParameterInfo> ExtractParameters(string codeBlock)
    {
        var parameters = new List<ParameterInfo>();
        
        if (string.IsNullOrWhiteSpace(codeBlock))
            return parameters;

        var lines = codeBlock.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.StartsWith("[Parameter]"))
            {
                // Look for the next line for the property declaration
                if (i + 1 < lines.Length)
                {
                    var propertyLine = lines[i + 1].Trim();
                    
                    // Parse: public Type Name { get; set; }
                    if (propertyLine.Contains("public") && propertyLine.Contains("{"))
                    {
                        var parts = propertyLine.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            var type = parts[1];
                            var name = parts[2];
                            parameters.Add(new ParameterInfo { Type = type, Name = name });
                        }
                    }
                }
            }
        }

        return parameters;
    }

    private string ExtractNamespace(string filePath)
    {
        // Razor components are generated in format: RootNamespace.SubPath
        // For Components/Greeting.razor -> RazorBlade.Sample.Components
        var parts = filePath.Replace('\\', '/').Split('/');
        var relevantParts = new List<string>();
        
        // Find the Components, Pages, or Shared directory and everything after it
        var componentsIndex = -1;
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "Components" || parts[i] == "Pages" || parts[i] == "Shared")
            {
                componentsIndex = i;
                break;
            }
        }

        // Build the namespace based on project structure
        // We'll use RazorBlade.Sample.Components as the namespace
        if (componentsIndex >= 0)
        {
            var sb = new StringBuilder();
            sb.Append("RazorBlade.Sample");
            
            for (int i = componentsIndex; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!string.IsNullOrEmpty(part) && !part.EndsWith(".razor"))
                {
                    sb.Append(".");
                    sb.Append(char.ToUpper(part[0]) + part.Substring(1));
                }
            }
            
            return sb.ToString();
        }

        return "RazorBlade.Sample.Components";
    }

    private string GenerateExtensionMethods(ComponentInfo component)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.AspNetCore.Components;");
        sb.AppendLine();
        sb.AppendLine("namespace RazorBlade.Extensions");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Extension methods for rendering {component.Name} components.");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public static class {component.Name}Extensions");
        sb.AppendLine("    {");

        // Generate factory method
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// Creates and renders a {component.Name} component with the specified parameters.");
        sb.AppendLine($"        /// </summary>");
        
        foreach (var param in component.Parameters)
        {
            sb.AppendLine($"        /// <param name=\"{ToCamelCase(param.Name)}\">The {param.Name} parameter.</param>");
        }
        
        sb.AppendLine("        /// <returns>The rendered HTML as a string.</returns>");
        sb.Append($"        public static async Task<string> Render{component.Name}Async(");
        
        var paramList = string.Join(", ", component.Parameters.Select(p => $"{p.Type} {ToCamelCase(p.Name)}"));
        sb.Append(paramList);
        sb.AppendLine(")");
        sb.AppendLine("        {");
        sb.AppendLine("            var parameters = new Dictionary<string, object?>");
        sb.AppendLine("            {");
        
        foreach (var param in component.Parameters)
        {
            sb.AppendLine($"                {{ \"{param.Name}\", {ToCamelCase(param.Name)} }},");
        }
        
        sb.AppendLine("            };");
        sb.AppendLine();
        sb.AppendLine($"            return await RazorBlade.ComponentRenderer.RenderComponentAsync<{component.Namespace}.{component.Name}>(parameters);");
        sb.AppendLine("        }");

        // Generate synchronous version
        sb.AppendLine();
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// Creates and renders a {component.Name} component synchronously with the specified parameters.");
        sb.AppendLine($"        /// </summary>");
        
        foreach (var param in component.Parameters)
        {
            sb.AppendLine($"        /// <param name=\"{ToCamelCase(param.Name)}\">The {param.Name} parameter.</param>");
        }
        
        sb.AppendLine("        /// <returns>The rendered HTML as a string.</returns>");
        sb.Append($"        public static string Render{component.Name}(");
        sb.Append(paramList);
        sb.AppendLine(")");
        sb.AppendLine("        {");
        sb.Append($"            return Render{component.Name}Async(");
        sb.Append(string.Join(", ", component.Parameters.Select(p => ToCamelCase(p.Name))));
        sb.AppendLine(").GetAwaiter().GetResult();");
        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        return char.ToLower(name[0]) + name.Substring(1);
    }

    private class ComponentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public List<ParameterInfo> Parameters { get; set; } = new();
    }

    private class ParameterInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
