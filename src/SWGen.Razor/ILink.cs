using SWGen.FileSystems;

namespace SWGen.Razor;

public interface ILink
{
    RelativePathEx BuildLink(IDocument doc);
}