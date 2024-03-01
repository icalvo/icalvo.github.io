using RazorLight.Razor;

namespace SWGen;

public class ViewImportsFileSystemRazorProject : FileSystemRazorProject
{
    private static readonly string[] DefaultImports = ["_ViewImports"];
    private readonly string[] _imports;
    private readonly ISwgLogger _logger;

    public ViewImportsFileSystemRazorProject(string root, ISwgLogger logger) : base(root)
    {
        _logger = logger;
        _imports = DefaultImports;
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, ISwgLogger logger) : base(root, extension)
    {
        _logger = logger;
        _imports = DefaultImports;
    }

    public ViewImportsFileSystemRazorProject(string root, string[] imports, ISwgLogger logger) : base(root)
    {
        _imports = imports;
        _logger = logger;
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, string[] imports, ISwgLogger logger) : base(root, extension)
    {
        _imports = imports;
        _logger = logger;
    }

    public override Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
    {
        var imports = GetImports(templateKey).ToArray();
        return Task.FromResult<IEnumerable<RazorLightProjectItem>>(imports);
    }

    private IEnumerable<FileSystemRazorProjectItem> GetImports(string templateKey)
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