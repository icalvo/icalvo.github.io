using FileSystemGenerator;

namespace CommandLine;

public static class PathDataExtensions
{
    public static string RelUrl(this PathData fd)
    {
        return fd.RelPath.Replace(@"\", "/", StringComparison.Ordinal);
    }
}
