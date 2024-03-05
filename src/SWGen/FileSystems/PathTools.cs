namespace SWGen.FileSystems;

public class PathTools
{
    private readonly IRawFileSystem _fileSystem;

    public PathTools(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string NormalizeFolderSeparator(AbsolutePathEx path) => _fileSystem.PathNormalizeFolderSeparator(path);
    public string NormalizeFolderSeparator(RelativePathEx path) => _fileSystem.PathNormalizeFolderSeparator(path);

    public string ReplaceForbiddenChars(string path, string replacement = "") =>
        _fileSystem.PathReplaceForbiddenChars(path, replacement);

    public Task<AbsolutePathEx> GetTempPath() => _fileSystem.PathGetTempPath();
}
