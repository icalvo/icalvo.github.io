namespace SWGen.FileSystems;

public class FileSystem(IRawFileSystem fileSystem) : IFileSystem
{
    public PathTools Path => new(fileSystem);
    public FileTools File => new(fileSystem);
    public DirectoryTools Directory => new(fileSystem);
    public bool IsReadOnly => fileSystem.IsReadOnly;
}
