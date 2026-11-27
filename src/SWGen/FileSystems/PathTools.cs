namespace SWGen.FileSystems;

public class PathTools(IRawFileSystem fileSystem)
{
    public string NormalizeFolderSeparator(AbsolutePathEx path) => fileSystem.PathNormalizeFolderSeparator(path);
    public string NormalizeFolderSeparator(RelativePathEx path) => fileSystem.PathNormalizeFolderSeparator(path);

    public string ReplaceForbiddenChars(string path, string replacement = "") =>
        fileSystem.PathReplaceForbiddenChars(path, replacement);

    public Task<AbsolutePathEx> GetTempPath() => fileSystem.PathGetTempPath();
}
