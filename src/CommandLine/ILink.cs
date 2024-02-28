using SWGen;

namespace CommandLine;

public interface ILink
{
    RelativePathEx BuildLink(IDocument doc);
}