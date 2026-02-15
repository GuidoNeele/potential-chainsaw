using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RazorBlade.SourceGenerator;

/// <summary>
/// Simple parser for .razor files with Razor Component syntax.
/// </summary>
public class RazorComponentParser
{
    public ParsedComponent Parse(string content, string filePath)
    {
        var component = new ParsedComponent
        {
            FilePath = filePath,
            RawContent = content
        };

        // Extract @code or @functions block
        component.CodeBlock = ExtractCodeBlock(content);
        
        // Extract parameters from code block
        component.Parameters = ExtractParameters(component.CodeBlock);
        
        // Extract the HTML/markup content (everything except the code block)
        component.MarkupContent = RemoveCodeBlock(content);
        
        // Extract @using directives
        component.Usings = ExtractUsings(content);
        
        // Extract @inherits directive (if any)
        component.BaseClass = ExtractInherits(content) ?? "RazorBlade.HtmlTemplate";
        
        return component;
    }

    private string ExtractCodeBlock(string content)
    {
        // Match @code { ... } or @functions { ... }
        var codePattern = @"@(?:code|functions)\s*\{";
        var match = Regex.Match(content, codePattern);
        
        if (!match.Success)
            return string.Empty;

        var startIndex = match.Index + match.Length;
        var braceCount = 1;
        var i = startIndex;

        while (i < content.Length && braceCount > 0)
        {
            if (content[i] == '{') braceCount++;
            else if (content[i] == '}') braceCount--;
            i++;
        }

        if (braceCount == 0)
        {
            return content.Substring(startIndex, i - startIndex - 1);
        }

        return string.Empty;
    }

    private string RemoveCodeBlock(string content)
    {
        // Remove @code or @functions block
        var codePattern = @"@(?:code|functions)\s*\{";
        var match = Regex.Match(content, codePattern);
        
        if (!match.Success)
            return content;

        var startIndex = match.Index;
        var braceStart = match.Index + match.Length;
        var braceCount = 1;
        var i = braceStart;

        while (i < content.Length && braceCount > 0)
        {
            if (content[i] == '{') braceCount++;
            else if (content[i] == '}') braceCount--;
            i++;
        }

        if (braceCount == 0)
        {
            return content.Substring(0, startIndex) + content.Substring(i);
        }

        return content;
    }

    private List<ComponentParameter> ExtractParameters(string codeBlock)
    {
        var parameters = new List<ComponentParameter>();
        
        if (string.IsNullOrWhiteSpace(codeBlock))
            return parameters;

        // Match [Parameter] followed by a property
        var paramPattern = @"\[Parameter\]\s+public\s+(\S+?)\s+(\w+)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*([^;]+);)?";
        var matches = Regex.Matches(codeBlock, paramPattern);

        foreach (Match match in matches)
        {
            parameters.Add(new ComponentParameter
            {
                Type = match.Groups[1].Value,
                Name = match.Groups[2].Value,
                DefaultValue = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
            });
        }

        return parameters;
    }

    private List<string> ExtractUsings(string content)
    {
        var usings = new List<string>();
        var usingPattern = @"@using\s+(.+)";
        var matches = Regex.Matches(content, usingPattern);

        foreach (Match match in matches)
        {
            usings.Add(match.Groups[1].Value.Trim());
        }

        return usings;
    }

    private string? ExtractInherits(string content)
    {
        var inheritsPattern = @"@inherits\s+(.+)";
        var match = Regex.Match(content, inheritsPattern);
        
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}

public class ParsedComponent
{
    public string FilePath { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public string CodeBlock { get; set; } = string.Empty;
    public string MarkupContent { get; set; } = string.Empty;
    public List<ComponentParameter> Parameters { get; set; } = new();
    public List<string> Usings { get; set; } = new();
    public string BaseClass { get; set; } = "RazorBlade.HtmlTemplate";
}

public class ComponentParameter
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}
