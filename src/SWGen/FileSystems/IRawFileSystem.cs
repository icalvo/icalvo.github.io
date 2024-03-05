using System.IO;

namespace SWGen.FileSystems;

public interface IRawFileSystem
{
    Task DirectoryCreateAsync(AbsolutePathEx path);
    Task<bool> FileExistsAsync(AbsolutePathEx path);
    Task FileWriteAsync(AbsolutePathEx destinationPath, Stream stream);
    Task FileWriteAllTextAsync(AbsolutePathEx filePath, string content);
    Task<Stream> FileOpenReadAsync(AbsolutePathEx filePath);
    Task<string> FileReadAllTextAsync(AbsolutePathEx filePath);
    string PathNormalizeFolderSeparator(AbsolutePathEx path);
    string PathNormalizeFolderSeparator(RelativePathEx path);
    bool IsReadOnly { get; }
    Task<bool> DirectoryExistsAsync(AbsolutePathEx path);
    string PathReplaceForbiddenChars(string path, string replacement = "");
    string DirectoryGetCurrent();
    IEnumerable<AbsolutePathEx> DirectoryGetFiles(AbsolutePathEx path, string pattern, EnumerationOptions options);
    Task FileMoveAsync(AbsolutePathEx sourceFile, AbsolutePathEx destinationFile, bool overwrite);
    Task<Stream> FileCreateAsync(AbsolutePathEx file);
    Task<AbsolutePathEx> PathGetTempPath();
}
