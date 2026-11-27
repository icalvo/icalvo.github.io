using System.IO;

namespace SWGen.FileSystems;

public class FileTools(IRawFileSystem fileSystem)
{
    public Task<bool> ExistsAsync(AbsolutePathEx path) => fileSystem.FileExistsAsync(path);
    public Task WriteAsync(AbsolutePathEx destinationPath, Stream stream) => fileSystem.FileWriteAsync(destinationPath, stream);
    public Task WriteAllTextAsync(AbsolutePathEx filePath, string content) => fileSystem.FileWriteAllTextAsync(filePath, content);
    public Task<Stream> OpenReadAsync(AbsolutePathEx filePath) => fileSystem.FileOpenReadAsync(filePath);
    public Task<string> ReadAllTextAsync(AbsolutePathEx filePath) => fileSystem.FileReadAllTextAsync(filePath);

    public Task MoveAsync(AbsolutePathEx sourceFile, AbsolutePathEx destinationFile, bool overwrite)
        => fileSystem.FileMoveAsync(sourceFile, destinationFile, overwrite);

    public Task<Stream> CreateAsync(AbsolutePathEx file)
    {
        return fileSystem.FileCreateAsync(file);
    }
}
