using System.IO;
using RazorLight.Razor;
using SWGen.FileSystems;

namespace SWGen.Razor;

public class MyFileSystemRazorProjectItem(string templateKey, AbsolutePathEx file, IFileSystem fs) : RazorLightProjectItem
{
    public AbsolutePathEx File { get; } = file;

    public override string Key { get; } = templateKey;

    public override bool Exists => fs.File.ExistsAsync(File).Result;

    public override Stream Read() => fs.File.OpenReadAsync(File).Result;
}
