using SWGen;

namespace CommandLine;

public class Page : ITitled, ICreatable<Page>
{
    public required string Title { get; init; }
    public required string Link { get; init; }
    public static Page Create()
    {
        return new Page { Title = "NO TITLE!", Link = "https://NOLINK.com" };
    }
}