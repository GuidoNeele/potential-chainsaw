using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.SourceGenerator;

[Generator]
public class RazorComponentSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get assembly name for namespace
        var assemblyName = context.CompilationProvider.Select((c, _) => c.AssemblyName ?? "GeneratedComponents");

        // Get all .razor files
        var razorFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".razor") && 
                          !file.Path.Contains("_Imports.razor") &&
                          !file.Path.Contains("App.razor"))
            .Select((file, cancellationToken) =>
            {
                var text = file.GetText(cancellationToken);
                return (Path: file.Path, Content: text?.ToString() ?? string.Empty);
            })
            .Collect();

        var combined = razorFiles.Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, data) =>
        {
            var (files, rootNamespace) = data;
            foreach (var file in files)
            {
                try
                {
                    GenerateComponent(spc, file.Path, file.Content, rootNamespace);
                }
                catch (Exception ex)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "RB001",
                            "Razor Component Generation Error",
                            $"Error generating component for {Path.GetFileName(file.Path)}: {ex.Message}",
                            "RazorBlade",
                            DiagnosticSeverity.Error,
                            true),
                        Location.None));
                }
            }
        });
    }

    private void GenerateComponent(SourceProductionContext context, string filePath, string content, string rootNamespace)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var namespaceName = ExtractNamespace(filePath, rootNamespace);

        // Parse the component
        var parser = new RazorComponentParser();
        var component = parser.Parse(content, filePath);

        // Generate C# code
        var generator = new RazorComponentCodeGenerator();
        var code = generator.Generate(component, fileName, namespaceName);

        // Add the generated code
        context.AddSource($"{fileName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private string ExtractNamespace(string filePath, string rootNamespace)
    {
        var parts = filePath.Replace('\\', '/').Split('/');
        var relevantParts = new List<string>();

        var componentsIndex = -1;
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "Components" || parts[i] == "Pages" || parts[i] == "Shared" || parts[i] == "Templates")
            {
                componentsIndex = i;
                break;
            }
        }

        if (componentsIndex >= 0)
        {
            var sb = new StringBuilder();
            sb.Append(rootNamespace);

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

        return rootNamespace;
    }
}
