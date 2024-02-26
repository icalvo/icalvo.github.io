using RazorLight.Razor;

namespace SWGen;

public class ViewImportsFileSystemRazorProject : FileSystemRazorProject
{
    private static readonly string[] DefaultImports = ["_ViewImports"];
    private readonly string[] _imports;
    private readonly Dictionary<string, string> _generationContent = new();

    public ViewImportsFileSystemRazorProject(string root) : base(root)
    {
        _imports = DefaultImports;
    }

    public ViewImportsFileSystemRazorProject(string root, string extension) : base(root, extension)
    {
        _imports = DefaultImports;
    }

    public ViewImportsFileSystemRazorProject(string root, string[] imports) : base(root)
    {
        _imports = imports;
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, string[] imports) : base(root, extension)
    {
        _imports = imports;
    }

    public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
    {
        if (_generationContent.TryGetValue(templateKey, out var content))
        {
            return Task.FromResult<RazorLightProjectItem>(new InMemoryRazorProjectItem(templateKey, content));
        }

        return base.GetItemAsync(templateKey);
    }
    
    public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
    {
        var imports = GetImports(templateKey).ToArray();
        Logger.Debug($"Imports for {templateKey}: {imports.Select(i => i.File).StringJoin(", ")}");
        return Task.FromResult<IEnumerable<RazorLightProjectItem>>(imports);
    }

    public IEnumerable<FileSystemRazorProjectItem> GetImports(string templateKey)
    {
        var filePath = AbsolutePathEx.Create(GetAbsoluteFilePathFromKey(templateKey));

        var startDirPath = filePath.Parent;

        foreach (var import in _imports)
        {
            if (startDirPath.GetFirstExistingFileInParentDirs(import + Extension) is { } file)
            {
                yield return new FileSystemRazorProjectItem(templateKey, new FileInfo(file.Normalized()));
            }
        }
    }

    public void RegisterContent(string key, string content)
    {
        _generationContent[key] = content;
    }
}

public static class EnumerableHelpers
{
    public static IEnumerable<T> Unfold<T>(this T seed, Func<T, bool> isLast, Func<T, T> generator)
    {
        var current = seed;
        while (true)
        {
            yield return current;
            if (isLast(current)) yield break;
            current = generator(current);
        }
    }
}
public static class AbsoluteFilePathExExtensions
{
    public static AbsolutePathEx? GetFirstExistingFileInParentDirs(this AbsolutePathEx startDirPath, string fileName)
    {
        return startDirPath.Unfold(x => x.IsRoot, x => x.Parent)
            .Select(dirPath => dirPath / fileName)
            .FirstOrDefault(x => File.Exists(x.Normalized()));
    }

}