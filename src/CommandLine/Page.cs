using SWGen;

namespace CommandLine;

public class Page : ITitled, ICreatable<Page>, ILink
{
    public required string Title { get; init; }
    public required RelativePathEx Link { get; init; }
    public static Page Create()
    {
        return new Page { Title = "NO TITLE!", Link = "NOLINK/index.html" };
    }

    public RelativePathEx BuildLink(IDocument doc) => Link;
}