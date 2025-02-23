using FileSystemGenerator;

namespace CommandLine;

[FileSystem("input")]
public static partial class InputFileSystem;

public static class PathDataExtensions
{
    public static string RelUrl(this PathData fd)
    {
        return fd.RelPath.Replace(@"\", "/");
    }
}