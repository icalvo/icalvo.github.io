namespace SWGen.FileSystems;

public interface IFileSystemFactory
{
    public Task<IFileSystem> BuildAsync(bool? readOnly = false);
    string FileSystemType { get; }
}
