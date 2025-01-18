using System.Text.RegularExpressions;
using SWGen.FileSystems;

namespace Tests;

public class MemoryRawFileSystem : IRawFileSystem
{
    private readonly Dictionary<string, FolderEntry> _roots;
    private readonly Dictionary<AbsolutePathEx, FolderEntry> _folders = new();
    private readonly Dictionary<AbsolutePathEx, FileEntry> _files = new();

    public MemoryRawFileSystem(params string[] roots)
    {
        _roots = roots.ToDictionary(x => x, _ => new FolderEntry("", null));
        if (_roots.Count == 0)
        {
            _roots.Add("", new FolderEntry("", null));
        }
    }
    
    public Task DirectoryCreateAsync(AbsolutePathEx path)
    {
        if (_folders.ContainsKey(path)) return Task.CompletedTask;

        if (!_roots.TryGetValue(path.Drive, out var current)) throw new Exception("Drive not found");

        var currentPath = AbsolutePathEx.Create(path.Drive);
        
        foreach (var part in path.Parts)
        {
            currentPath = currentPath / part;

            if (current.Children.TryGetValue(part, out var child))
            {
                current = child as FolderEntry ?? throw new Exception("Path already exists as a file");
            }
            else
            {
                var newFolder = new FolderEntry(part, current);
                current.Children.Add(part, newFolder);
                current = newFolder;
                _folders.Add(currentPath, newFolder);
            }
        }        

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(AbsolutePathEx path)
    {
        return Task.FromResult(_files.ContainsKey(path));
    }

    public Task FileWriteAsync(AbsolutePathEx destinationPath, Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task FileWriteAllTextAsync(AbsolutePathEx filePath, string content)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> FileOpenReadAsync(AbsolutePathEx filePath)
    {
        throw new NotImplementedException();
    }

    public Task<string> FileReadAllTextAsync(AbsolutePathEx filePath)
    {
        throw new NotImplementedException();
    }

    public string PathNormalizeFolderSeparator(AbsolutePathEx path)
    {
        return path.Normalized('\\');
    }

    public string PathNormalizeFolderSeparator(RelativePathEx path)
    {
        return path.Normalized('\\');
    }

    public bool IsReadOnly => false;

    public Task<bool> DirectoryExistsAsync(AbsolutePathEx path)
    {
        return Task.FromResult(_folders.ContainsKey(path));
    }

    public string PathReplaceForbiddenChars(string path, string replacement = "")
    {
        return path;
    }

    public string DirectoryGetCurrent()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<AbsolutePathEx> DirectoryGetFiles(AbsolutePathEx path, string pattern, EnumerationOptions options)
    {
        var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.Compiled);
        if (!_folders.TryGetValue(path, out var root)) throw new Exception("Path not found");
        
        return root.Children.Values.SelectMany(
            fse => fse switch
            {
                FileEntry fileEntry => regex.IsMatch(fileEntry.Name)
                    ? [path / fileEntry.Name]
                    : Array.Empty<AbsolutePathEx>(),
                FolderEntry folderEntry => options.RecurseSubdirectories
                    ? DirectoryGetFiles(path / folderEntry.Name, pattern, options)
                    : Array.Empty<AbsolutePathEx>(),
                _ => throw new ArgumentOutOfRangeException(nameof(fse))
            });
    }

    public Task FileMoveAsync(AbsolutePathEx sourceFile, AbsolutePathEx destinationFile, bool overwrite)
    {
        if (!_files.Remove(sourceFile, out var file)) throw new Exception("Source file not found");
        if (!_folders.TryGetValue(destinationFile.Parent, out var destinationFolder)) throw new Exception("Destination folder not found");

        var newFile = new FileEntry(destinationFile.FileName, file.Content, destinationFolder);
        _files.Add(destinationFile, newFile);
        return Task.CompletedTask;
    }

    public Task<Stream> FileCreateAsync(AbsolutePathEx file)
    {
        if (!_folders.TryGetValue(file.Parent, out var destinationFolder)) throw new Exception("Destination folder not found");
        _files[file] = new FileEntry(file.FileName, destinationFolder);
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task<AbsolutePathEx> PathGetTempPath()
    {
        throw new NotImplementedException();
    }
    

    private abstract record FileSystemEntry {}

    private record FolderEntry : FileSystemEntry
    {
        public FolderEntry(string name, FolderEntry? parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; }
        public FolderEntry? Parent { get; }
        public Dictionary<string, FileSystemEntry> Children { get; } = new();
    }

    private record FileEntry : FileSystemEntry
    {
        public FileEntry(string name, byte[] content, FolderEntry parent)
        {
            Name = name;
            Content = content;
            Parent = parent;
            Parent.Children.Add(name, this);

        }
        public FileEntry(string name, FolderEntry parent) : this (name, [], parent)
        {
        }

        public string Name { get; }
        public byte[] Content { get; }
        public FolderEntry Parent { get; }
    }
}