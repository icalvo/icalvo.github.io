using System.IO;
using RazorLight.Razor;
using SWGen.FileSystems;

namespace SWGen;

public class ViewImportsFileSystemRazorProject : FileSystemRazorProject
{
    private static readonly string[] DefaultImports = ["_ViewImports"];
    private readonly string[] _imports;
    private readonly IFileSystem _fs;
    public ViewImportsFileSystemRazorProject(string root, IFileSystem fs) : base(root)
    {
        _fs = fs;
        _imports = DefaultImports;
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, IFileSystem fs) : base(root, extension)
    {
        _fs = fs;
        _imports = DefaultImports;
    }

    public ViewImportsFileSystemRazorProject(string root, string[] imports, IFileSystem fs) : base(root)
    {
        _imports = imports;
        _fs = fs;
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, string[] imports, IFileSystem fs) : base(root, extension)
    {
        _imports = imports;
        _fs = fs;
    }

    public override async Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        => await GetImports(templateKey).ToArrayAsync();

    private async IAsyncEnumerable<FileSystemRazorProjectItem> GetImports(string templateKey)
    {
        var filePath = AbsolutePathEx.Create(GetAbsoluteFilePathFromKey(templateKey));

        var startDirPath = filePath.Parent;

        foreach (var import in _imports)
        {
            if (await startDirPath.GetFirstExistingFileInParentDirs(_fs, import + Extension) is { } file)
            {
                yield return new FileSystemRazorProjectItem(templateKey, new FileInfo(file.Normalized(_fs)));
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
    public static ValueTask<AbsolutePathEx?> GetFirstExistingFileInParentDirs(this AbsolutePathEx startDirPath, IFileSystem fs, string fileName)
    {
        return startDirPath.Unfold(x => x.IsRoot, x => x.Parent)
            .Select(dirPath => dirPath / fileName)
            .ToAsyncEnumerable()
            .FirstOrDefaultAwaitAsync(async x => await fs.File.ExistsAsync(x));
    }

}