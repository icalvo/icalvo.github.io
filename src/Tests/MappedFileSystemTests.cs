using CommandLine;
using SWGen.FileSystems;

namespace Tests;

public class MappedFileSystemTests {
    [Fact]
    public async Task Test()
    {
        var fs = new MemoryRawFileSystem("C:", "D:");
        
        await fs.DirectoryCreateAsync(@"C:\dir1\dir2\dir3");
        await fs.DirectoryCreateAsync(@"D:\dir5\dir6");
        _ = await fs.FileCreateAsync(@"D:\dir5\dir6\file1.txt");
        _ = await fs.FileCreateAsync(@"C:\dir1\file2.txt");
        _ = await fs.FileCreateAsync(@"C:\dir1\dir2\file3.txt");
        Assert.True(await fs.DirectoryExistsAsync(@"C:\dir1\dir2\dir3"));

        var mfs = new MappedFileSystem(fs);
        mfs.AddMapping(@"C:\dir1\dir2", @"D:\dir5\dir6");

        var files = fs.DirectoryGetFiles(@"C:\dir1", "*.txt", new EnumerationOptions { RecurseSubdirectories = true })
            .Select(x => x.Normalized(fs))
            .ToArray();
        Assert.Equal([@"C:\dir1\dir2\file3.txt", @"C:\dir1\file2.txt"], files.OrderBy(x => x));
        
        files = mfs.DirectoryGetFiles(@"C:\dir1", "*.txt", new EnumerationOptions { RecurseSubdirectories = true })
            .Select(x => x.Normalized(fs))
            .ToArray();
        Assert.Equal([@"C:\dir1\dir2\file1.txt", @"C:\dir1\file2.txt"], files.OrderBy(x => x));
    }
}