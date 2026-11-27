using System.IO;

namespace SWGen.FileSystems;

public class MappedFileSystem(IRawFileSystem rawFileSystemImplementation) : IRawFileSystem
{
    private readonly List<Mapping> _mappings = [];

    public void AddMapping(AbsolutePathEx origin, AbsolutePathEx destination)
    {
        _mappings.Add(new Mapping(origin, destination));
    }

    public Task DirectoryCreateAsync(AbsolutePathEx path)
    {
        return rawFileSystemImplementation.DirectoryCreateAsync(path);
    }

    public Task<bool> FileExistsAsync(AbsolutePathEx path)
    {
        return rawFileSystemImplementation.FileExistsAsync(path);
    }

    public Task FileWriteAsync(AbsolutePathEx destinationPath, Stream stream)
    {
        return rawFileSystemImplementation.FileWriteAsync(destinationPath, stream);
    }

    public Task FileWriteAllTextAsync(AbsolutePathEx filePath, string content)
    {
        return rawFileSystemImplementation.FileWriteAllTextAsync(filePath, content);
    }

    public Task<Stream> FileOpenReadAsync(AbsolutePathEx filePath)
    {
        return rawFileSystemImplementation.FileOpenReadAsync(filePath);
    }

    public Task<string> FileReadAllTextAsync(AbsolutePathEx filePath)
    {
        return rawFileSystemImplementation.FileReadAllTextAsync(filePath);
    }

    public string PathNormalizeFolderSeparator(AbsolutePathEx path)
    {
        return rawFileSystemImplementation.PathNormalizeFolderSeparator(MappedPath(path));
    }

    private AbsolutePathEx MappedPath(AbsolutePathEx path) => GetMapping(path) is { } m ? m.Destination / m.RelativePath : path;

    private bool IsMappedPath(AbsolutePathEx path) => GetMapping(path) != null;

    private MappingResult? GetMapping(AbsolutePathEx path) =>
        _mappings
            .Select(mapping =>
            {
                var rel = path.RelativeTo(mapping.Origin);
                return rel == null ? null : new MappingResult(mapping.Destination, rel);
            })
            .FirstOrDefault(x => x != null);

    private IEnumerable<Mapping> MappingDescendants(AbsolutePathEx path) =>
        _mappings.Where(mapping => mapping.Origin.IsChildOrSame(path));

    public string PathNormalizeFolderSeparator(RelativePathEx path)
    {
        return rawFileSystemImplementation.PathNormalizeFolderSeparator(path);
    }

    public bool IsReadOnly => rawFileSystemImplementation.IsReadOnly;

    public Task<bool> DirectoryExistsAsync(AbsolutePathEx path)
    {
        return rawFileSystemImplementation.DirectoryExistsAsync(path);
    }

    public string PathReplaceForbiddenChars(string path, string replacement = "")
    {
        return rawFileSystemImplementation.PathReplaceForbiddenChars(path, replacement);
    }

    public string DirectoryGetCurrent()
    {
        return rawFileSystemImplementation.DirectoryGetCurrent();
    }

    public IEnumerable<AbsolutePathEx> DirectoryGetFiles(AbsolutePathEx path, string pattern, EnumerationOptions options)
    {
        var destinationFiles =
            MappingDescendants(path)
                .SelectMany(mapping => rawFileSystemImplementation.DirectoryGetFiles(mapping.Destination, pattern, options)
                    .Select(destPath =>
                    {
                        var rel = destPath.RelativeTo(mapping.Destination)
                            ?? throw new InvalidOperationException("Mapped file is not under mapping destination");
                        return mapping.Origin / rel;
                    }));

        var originFiles =
            _mappings.Any(m => path.IsChildOrSame(m.Origin))
            ? []
            : rawFileSystemImplementation.DirectoryGetFiles(path, pattern, options);

        return originFiles.Where(f => !IsMappedPath(f))
            .Concat(destinationFiles);
    }

    public Task FileMoveAsync(AbsolutePathEx sourceFile, AbsolutePathEx destinationFile, bool overwrite)
    {
        return rawFileSystemImplementation.FileMoveAsync(sourceFile, destinationFile, overwrite);
    }

    public Task<Stream> FileCreateAsync(AbsolutePathEx file)
    {
        return rawFileSystemImplementation.FileCreateAsync(file);
    }

    public Task<AbsolutePathEx> PathGetTempPath()
    {
        return rawFileSystemImplementation.PathGetTempPath();
    }
}
