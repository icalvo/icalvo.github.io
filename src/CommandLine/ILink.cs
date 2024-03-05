using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public interface ILink
{
    RelativePathEx BuildLink(IDocument doc);
}