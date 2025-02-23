using Generators;

namespace FileSystemSourceGenerator.Sample;

[FileSystem("bin")]
public static partial class BinDir;

public class Client
{
    public void M()
    {
        var x = BinDir.Debug.Net9_0.FileSystemSourceGenerator_Sample_dll;
    }
}
