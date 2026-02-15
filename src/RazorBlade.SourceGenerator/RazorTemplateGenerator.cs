using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RazorBlade.SourceGenerator;

[Generator]
public class RazorTemplateGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get assembly name for namespace
        var assemblyName = context.CompilationProvider.Select((c, _) => c.AssemblyName ?? "GeneratedTemplates");

        // Get all .razor files
        var razorFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".razor") && !file.Path.Contains("_Imports.razor"))
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
                    GenerateTemplate(spc, file.Path, file.Content, rootNamespace);
                }
                catch (Exception ex)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "RB001",
                            "Razor Template Generation Error",
                            $"Error generating template for {Path.GetFileName(file.Path)}: {ex.Message}",
                            "RazorBlade",
                            DiagnosticSeverity.Error,
                            true),
                        Location.None));
                }
            }
        });
    }

    private void GenerateTemplate(SourceProductionContext context, string filePath, string content, string rootNamespace)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var namespaceName = ExtractNamespace(filePath, rootNamespace);

        // Configure Razor engine
        var projectEngine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(Path.GetDirectoryName(filePath) ?? "."),
            builder =>
            {
                builder.SetNamespace(namespaceName);
                builder.SetBaseType("RazorBlade.HtmlTemplate");
                builder.AddDefaultImports("@using System",
                    "@using System.Collections.Generic",
                    "@using System.Linq",
                    "@using System.Threading.Tasks",
                    "@using RazorBlade");
            });

        // Create Razor document
        var document = RazorSourceDocument.Create(content, Path.GetFileName(filePath));

        // Generate C# code
        var codeDocument = projectEngine.Process(document, null, new List<RazorSourceDocument>(), new List<TagHelperDescriptor>());
        
        var csharpDocument = codeDocument.GetCSharpDocument();

        if (csharpDocument.Diagnostics.Any())
        {
            foreach (var diagnostic in csharpDocument.Diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "RB002",
                        "Razor Compilation Error",
                        $"{diagnostic.GetMessage()}",
                        "RazorBlade",
                        diagnostic.Severity == RazorDiagnosticSeverity.Error ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning,
                        true),
                    Location.None));
            }

            if (csharpDocument.Diagnostics.Any(d => d.Severity == RazorDiagnosticSeverity.Error))
                return;
        }

        // Add the generated C# code as a source file
        var generatedCode = csharpDocument.GeneratedCode;
        
        // Post-process the generated code to ensure it works with our base classes
        generatedCode = PostProcessGeneratedCode(generatedCode, fileName, namespaceName);

        context.AddSource($"{fileName}.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
    }

    private string PostProcessGeneratedCode(string code, string className, string namespaceName)
    {
        // The Razor compiler generates code with some quirks we need to fix
        // Make the class partial and ensure proper inheritance
        code = code.Replace($"public class {className}", $"public partial class {className}");
        
        return code;
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
