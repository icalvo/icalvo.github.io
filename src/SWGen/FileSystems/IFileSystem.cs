namespace SWGen.FileSystems;

public interface IFileSystem
{
    PathTools Path { get; }
    FileTools File { get; }
    DirectoryTools Directory { get; }
    bool IsReadOnly { get; }
}
