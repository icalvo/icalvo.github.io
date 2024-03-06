using System.IO;
using RazorLight.Razor;
using SWGen.FileSystems;
using Microsoft.Extensions.Primitives;

namespace SWGen;
public class MyFileSystemRazorProjectItem : RazorLightProjectItem
{
    private readonly IFileSystem _fs;

    public MyFileSystemRazorProjectItem(string templateKey, AbsolutePathEx file, IFileSystem fs)
    {
        _fs = fs;
        Key = templateKey;
        File = file;
    }

    public AbsolutePathEx File { get; }

    public override string Key { get; }

    public override bool Exists => _fs.File.ExistsAsync(File).Result;

    public override Stream Read() => (Stream) _fs.File.OpenReadAsync(File).Result;
}

/// <summary>
/// Specifies RazorProject where templates are located in files
/// </summary>
public class ViewImportsFileSystemRazorProject : RazorLightProject
{
    public const string DefaultExtension = ".cshtml";

    private readonly IFileSystem _fs;
    private static readonly string[] DefaultImports = ["_ViewImports"];
    private readonly string[] _imports;

    public ViewImportsFileSystemRazorProject(string root, IFileSystem fs)
    {
        _fs = fs;
        _imports = DefaultImports;
        Extension = DefaultExtension;
        Root = fs.Directory.ExistsAsync(root).Result ? root : throw new DirectoryNotFoundException("Root directory " + root + " not found");
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, IFileSystem fs)
    {
        _fs = fs;
        _imports = DefaultImports;
        Extension = extension ?? throw new ArgumentNullException(nameof (extension));
        Root = fs.Directory.ExistsAsync(root).Result ? root : throw new DirectoryNotFoundException("Root directory " + root + " not found");
    }

    public ViewImportsFileSystemRazorProject(string root, string[] imports, IFileSystem fs)
    {
        _imports = imports;
        _fs = fs;
        Extension = DefaultExtension;
        Root = fs.Directory.ExistsAsync(root).Result ? root : throw new DirectoryNotFoundException("Root directory " + root + " not found");
    }

    public ViewImportsFileSystemRazorProject(string root, string extension, string[] imports, IFileSystem fs)
    {
        _imports = imports;
        _fs = fs;
        Extension = extension ?? throw new ArgumentNullException(nameof (extension));
        Root = fs.Directory.ExistsAsync(root).Result ? root : throw new DirectoryNotFoundException("Root directory " + root + " not found");
    }

    public string Extension { get; set; }

    /// <summary>Root folder</summary>
    public AbsolutePathEx Root { get; }

    /// <summary>
    /// Looks up for the template source with a given <paramref name="templateKey" />
    /// </summary>
    /// <param name="templateKey">Unique template key</param>
    /// <returns></returns>
    public override Task<RazorLightProjectItem> GetItemAsync(string templateKey)
    {
      if (!templateKey.EndsWith(Extension))
        templateKey += Extension;
      var absoluteFilePathFromKey = GetAbsoluteFilePathFromKey(templateKey);
      RazorLightProjectItem result = new MyFileSystemRazorProjectItem(templateKey, absoluteFilePathFromKey, _fs);
      if (result.Exists)
          result.ExpirationToken = new CancellationChangeToken(CancellationToken.None);
      //_fileProvider.Watch(templateKey);
      return Task.FromResult(result);
    }

    protected AbsolutePathEx GetAbsoluteFilePathFromKey(string templateKey)
    {
      if (string.IsNullOrEmpty(templateKey)) throw new ArgumentNullException(nameof (templateKey));
      var tp = PathEx.Create(templateKey);
      return tp as AbsolutePathEx ?? Root / templateKey;
    }

    public override Task<IEnumerable<string>> GetKnownKeysAsync()
    {
      return Task.FromResult(
          _fs.Directory.GetFiles(Root, "*" + Extension, new EnumerationOptions { RecurseSubdirectories = true })
          .Select(f => f.Normalized(_fs)));
    }

    public override string NormalizeKey(string templateKey)
    {
      return GetAbsoluteFilePathFromKey(templateKey).RelativeTo(Root)!.Normalized(_fs);
    }


    public override async Task<IEnumerable<RazorLightProjectItem>> GetImportsAsync(string templateKey)
        => await GetImports(templateKey).ToArrayAsync();

    private async IAsyncEnumerable<FileSystemRazorProjectItem> GetImports(string templateKey)
    {
        var filePath = GetAbsoluteFilePathFromKey(templateKey);

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