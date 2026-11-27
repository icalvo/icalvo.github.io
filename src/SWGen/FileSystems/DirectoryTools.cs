using System.IO;

namespace SWGen.FileSystems;

public class DirectoryTools(IRawFileSystem fileSystem)
{
    public async Task CreateIfNotExistAsync(AbsolutePathEx path)
    {
        if (!await fileSystem.DirectoryExistsAsync(path))
        {
            await fileSystem.DirectoryCreateAsync(path);
        }
    }

    public Task CreateAsync(AbsolutePathEx path) => fileSystem.DirectoryCreateAsync(path);
    public Task<bool> ExistsAsync(AbsolutePathEx path) => fileSystem.DirectoryExistsAsync(path);

    public string GetCurrentDirectory() => fileSystem.DirectoryGetCurrent();

    public IEnumerable<AbsolutePathEx> GetFiles(AbsolutePathEx path, string pattern, EnumerationOptions options)
    {
        return fileSystem.DirectoryGetFiles(path, pattern, options);
    }
}
