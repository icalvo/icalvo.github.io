using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class AudioInfo
{
    public required RelativePathEx WorkDir { get; init; }
    public required string MovementKey { get; init; }
    public required AbsolutePathEx ProjectRoot { get; init; }
    public required IFileSystem Fs { get; init; }
}