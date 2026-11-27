using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using RazorLight.Razor;
using SWGen.FileSystems;

namespace SWGen.Razor;

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
      if (!templateKey.EndsWith(Extension, StringComparison.Ordinal))
        templateKey += Extension;
      var absoluteFilePathFromKey = GetAbsoluteFilePathFromKey(templateKey);
      RazorLightProjectItem result = new MyFileSystemRazorProjectItem(templateKey, absoluteFilePathFromKey, _fs);
      if (result.Exists)
          result.ExpirationToken = new CancellationChangeToken(CancellationToken.None);
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
      var relative = GetAbsoluteFilePathFromKey(templateKey).RelativeTo(Root)
          ?? throw new InvalidOperationException($"Template key '{templateKey}' is not under project root '{Root}'");
      return relative.Normalized(_fs);
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
