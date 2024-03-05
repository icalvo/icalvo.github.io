using System.IO;

namespace SWGen.FileSystems;

public class FileTools
{
    private readonly IRawFileSystem _fileSystem;

    public FileTools(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Task<bool> ExistsAsync(AbsolutePathEx path) => _fileSystem.FileExistsAsync(path);
    public Task WriteAsync(AbsolutePathEx destinationPath, Stream stream) => _fileSystem.FileWriteAsync(destinationPath, stream);
    public Task WriteAllTextAsync(AbsolutePathEx filePath, string content) => _fileSystem.FileWriteAllTextAsync(filePath, content);
    public Task<Stream> OpenReadAsync(AbsolutePathEx filePath) => _fileSystem.FileOpenReadAsync(filePath);
    public Task<string> ReadAllTextAsync(AbsolutePathEx filePath) => _fileSystem.FileReadAllTextAsync(filePath);

    public Task MoveAsync(AbsolutePathEx sourceFile, AbsolutePathEx destinationFile, bool overwrite)
        => _fileSystem.FileMoveAsync(sourceFile, destinationFile, overwrite);

    public Task<Stream> CreateAsync(AbsolutePathEx file)
    {
        return _fileSystem.FileCreateAsync(file);
    }
}