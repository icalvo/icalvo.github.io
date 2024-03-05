using System.IO;

namespace SWGen.FileSystems;

public class LocalFileSystem : IRawFileSystem
{
    public Task DirectoryCreateAsync(AbsolutePathEx path) => TaskEx.CompletedTask(() => Directory.CreateDirectory(Norm(path)));

    public Task<bool> FileExistsAsync(AbsolutePathEx path) => Task.FromResult(File.Exists(Norm(path)));

    public async Task FileWriteAsync(AbsolutePathEx destinationPath, Stream stream)
    {
        await using var outputStream = File.Open(Norm(destinationPath), FileMode.Create);
        await stream.CopyToAsync(outputStream);
    }

    public Task FileWriteAllTextAsync(AbsolutePathEx filePath, string content) =>
        File.WriteAllTextAsync(Norm(filePath), content);

    public Task<Stream> FileOpenReadAsync(AbsolutePathEx filePath) => Task.FromResult((Stream)File.OpenRead(Norm(filePath)));

    public Task<string> FileReadAllTextAsync(AbsolutePathEx filePath) => File.ReadAllTextAsync(Norm(filePath));
    public string PathNormalizeFolderSeparator(AbsolutePathEx path) => path.Normalized(Path.DirectorySeparatorChar);
    private string Norm(AbsolutePathEx path) => PathNormalizeFolderSeparator(path);
    public string PathNormalizeFolderSeparator(RelativePathEx path) => path.Normalized(Path.DirectorySeparatorChar);

    public bool IsReadOnly => false;

    public Task<bool> DirectoryExistsAsync(AbsolutePathEx path) => Task.FromResult(Directory.Exists(Norm(path)));

    public string PathReplaceForbiddenChars(string path, string replacement = "") =>
        Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).Aggregate(
            path,
            (p, invalidChar) => p.Replace(invalidChar.ToString(), replacement));

    public string DirectoryGetCurrent()
    {
        return Directory.GetCurrentDirectory();
    }

    public IEnumerable<AbsolutePathEx> DirectoryGetFiles(AbsolutePathEx path, string pattern, EnumerationOptions options)
    {
        return Directory.GetFiles(Norm(path), pattern, options)
            .Select(AbsolutePathEx.Create);
    }

    public Task FileMoveAsync(AbsolutePathEx sourceFile, AbsolutePathEx destinationFile, bool overwrite)
    {
        return TaskEx.CompletedTask(() => File.Move(Norm(sourceFile), Norm(destinationFile), overwrite));
    }

    public Task<Stream> FileCreateAsync(AbsolutePathEx file)
    {
        return Task.FromResult((Stream)File.Create(Norm(file)));
    }

    public Task<AbsolutePathEx> PathGetTempPath()
    {
        return Task.FromResult<AbsolutePathEx>(Path.GetTempPath());
    }
}
