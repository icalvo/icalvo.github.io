using SWGen.FileSystems;

namespace CommandLine;

public static class WorkFiles
{
    public static AbsolutePathEx? Find(
        IFileSystem fs,
        AbsolutePathEx projectRoot,
        RelativePathEx workDir,
        string fileName)
    {
        return fs.Directory
            .GetFiles(projectRoot / workDir, fileName, new EnumerationOptions { RecurseSubdirectories = true })
            .FirstOrDefault();
    }

    public static Uri? Url(
        IFileSystem fs,
        AbsolutePathEx projectRoot,
        RelativePathEx workDir,
        string fileName)
    {
        var abs = Find(fs, projectRoot, workDir, fileName);
        return abs?.RelativeTo(projectRoot)?.Url();
    }

    public static string Mp3FileName(string movementKey) =>
        movementKey.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
            ? movementKey
            : movementKey + ".mp3";
}
