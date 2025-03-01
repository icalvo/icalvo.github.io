using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RS1035

namespace FileSystemSourceGenerator;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public class FileSystemIncrementalSourceGenerator : IIncrementalGenerator
{
    private const string Namespace = "FileSystemGenerator";
    private const string AttributeName = "FileSystemAttribute";

    private const string AttributeSourceCode =
        $$"""
        // <auto-generated/>
        #pragma warning disable CS9113
        
        namespace {{Namespace}}
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class {{AttributeName}}(params string[] RelativePaths) : System.Attribute
            {
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var projectDir =
            context.AnalyzerConfigOptionsProvider
                .Select((optionsProvider, _) =>
                    optionsProvider.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir)
                        ? projectDir
                        : null);
        // Add the marker attribute to the compilation.
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "FileSystemAttribute.g.cs",
            SourceText.From(AttributeSourceCode, Encoding.UTF8)));

        // Filter classes annotated with the marker attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.attributeFound)
            .Select((t, _) => (t.Item1, t.relativePaths));

        provider.Combine(projectDir);
        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()).Combine(projectDir),
            (ctx, tuple) => GenerateCode(ctx, tuple.Left.Left, tuple.Left.Right, tuple.Right));
    }

    /// <summary>
    /// Checks whether the Node is annotated with the marker attribute and maps syntax context to the specific node type (ClassDeclarationSyntax).
    /// </summary>
    /// <param name="context">Syntax context, based on CreateSyntaxProvider predicate</param>
    /// <returns>The specific cast and whether the attribute was found.</returns>
    private static (ClassDeclarationSyntax, string[] relativePaths, bool attributeFound) GetClassDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue; // if we can't get the symbol, ignore it

            var relativePaths =
                (attributeSyntax.ArgumentList?.Arguments ?? [])
                .SelectMany(argumentSyntax =>
                {
                    var argument = argumentSyntax.Expression;
                    var val = context.SemanticModel.GetConstantValue(argument).Value?.ToString();
                    return val == null ? Array.Empty<string>() : new [] {val};
                }).ToArray();
                    
            string attributeName = attributeSymbol.ContainingType.ToDisplayString();

            // Check the full name of the marker attribute.
            if (attributeName == $"{Namespace}.{AttributeName}")
                return (classDeclarationSyntax, relativePaths, true);
        }

        return (classDeclarationSyntax, [], false);
    }

    private static void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<(ClassDeclarationSyntax, string[])> classDeclarations, string? projectDir)
    {
        if (projectDir == null) return;

        // Go through all filtered class declarations.
        foreach (var (classDeclarationSyntax, relPaths) in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var className = classDeclarationSyntax.Identifier.Text;
            var dirs = relPaths.Select(relPath => Path.Combine(projectDir, relPath));

            string source =
                $$"""
                  using FileSystemGenerator;
                  
                  namespace {{namespaceName}}
                  {
                      public static partial class {{className}} {
                          {{GeneratePaths(dirs)}}
                      }
                  }
                  """;
            
            // Add the source code to the compilation.
            context.AddSource($"{className}.g.cs", source);        
            
        }
        
        string fileDataSource =
            $$"""
              namespace {{Namespace}}
              {
                  public record PathData(string RelPath, string AbsPath);
              }
              """;
            
        // Add the source code to the compilation.
        context.AddSource($"FileData.g.cs", fileDataSource);        

    }

    private static readonly Regex IdentifierRegex = new ("[^0-9a-zA-Z_]", RegexOptions.Compiled);
    private static string CleanIdentifier(string text)
    {
        var removeInvalidChars = IdentifierRegex.Replace(text, "_");
        var numberStart =
            removeInvalidChars[0].IsDigit()
                ? "_" + removeInvalidChars
                : removeInvalidChars;
        return numberStart.Substring(0, 1).ToUpperInvariant() + numberStart.Substring(1);
    }

    private static string AccessTags(bool isStatic) => isStatic ? "public static" : "public";

    private static string GetDirField(string dirType, string dirField, bool isStatic) =>
        $"{AccessTags(isStatic)} {dirType} {dirField} = new();";
    private static string AddRandomness(string text) => text + Guid.NewGuid().ToString("N");
    
    private static string GenerateDirectory(string dirType, string fileFields, string subdirFields) =>
        $$"""
              public class {{dirType}}
              {
                  {{fileFields}}
                  {{subdirFields}}
              }
          """;

    private class DirGeneration
    {
        public string DirClass { get; }
        public string DirField { get; }
        public string SubDirClasses { get; }

        public DirGeneration(string dirClass, string dirField, string subDirClasses)
        {
            DirClass = dirClass;
            DirField = dirField;
            SubDirClasses = subDirClasses;
        }

        public string AllClasses => DirClass + "\n" + SubDirClasses;
    }
   
    private static DirGeneration GenerateAbsolutePathsAux(DirectoryInfo di, Func<FileSystemInfo, string> getPathDataField, bool firstLevel = false)
    {
        var files = di.GetFiles();
        var directories = di.GetDirectories();
        var fileFields = files.Select(fi => getPathDataField(fi)).Append(getPathDataField(di)).StrJoin("\n");

        var lowerDirsGeneration = directories.Select(x => GenerateAbsolutePathsAux(x, getPathDataField)).ToArray();
        var currentSubdirClasses =
            lowerDirsGeneration
                .Select(x => x.AllClasses)
                .StrJoin("\n");
        var currentDirType = AddRandomness(CleanIdentifier(di.Name));
        var currentDirFieldName = CleanIdentifier(di.Name);
        var currentDirField = GetDirField(currentDirType, currentDirFieldName, firstLevel);
        var subDirFields =
            lowerDirsGeneration
                .Select(x => x.DirField)
                .StrJoin("\n");
        var currentClass = GenerateDirectory(currentDirType, fileFields, subDirFields);

        return new(currentClass, currentDirField, currentSubdirClasses);
    }

    private static string Render(DirGeneration x) =>
        $"""
        {x.SubDirClasses}
        {x.DirClass}
        {x.DirField}
        """;

    private static string GeneratePaths(IEnumerable<string> roots) =>
        roots
            .Select(root =>
            {
                return Render(
                    GenerateAbsolutePathsAux(new DirectoryInfo(root), GetPathDataField, firstLevel: true));
                string GetPathDataFieldName(FileSystemInfo fsi) =>
                    fsi switch
                    {
                        FileInfo f => CleanIdentifier(f.Name),
                        _ => "PathData",
                    };
                string GetPathDataField(FileSystemInfo f) =>
                    $"public readonly PathData {GetPathDataFieldName(f)} = new (\"{f.FullName.Substring(root.Length).Replace(@"\", @"\\")}\", \"{f.FullName.Replace(@"\", @"\\")}\");";
            })
            .StrJoin("\n");
}

public static class CharExtensions
{
    public static string StrJoin<T>(this IEnumerable<T> source, string separator) =>
        string.Join(separator, source);

    public static bool IsDigit(this char c) => c is >= '0' and <= '9';
}
