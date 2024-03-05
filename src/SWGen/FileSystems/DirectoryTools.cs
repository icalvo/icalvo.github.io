using System.IO;

namespace SWGen.FileSystems;

public class DirectoryTools
{
    private readonly IRawFileSystem _fileSystem;

    public DirectoryTools(IRawFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task CreateIfNotExistAsync(AbsolutePathEx path)
    {
        if (!await _fileSystem.DirectoryExistsAsync(path))
        {
            await _fileSystem.DirectoryCreateAsync(path);
        }
    }

    public Task CreateAsync(AbsolutePathEx path) => _fileSystem.DirectoryCreateAsync(path);
    public Task<bool> ExistsAsync(AbsolutePathEx path) => _fileSystem.DirectoryExistsAsync(path);

    public string GetCurrentDirectory() => _fileSystem.DirectoryGetCurrent();

    public IEnumerable<AbsolutePathEx> GetFiles(AbsolutePathEx path, string pattern, EnumerationOptions options)
    {
        return _fileSystem.DirectoryGetFiles(path, pattern, options);
    }
}
