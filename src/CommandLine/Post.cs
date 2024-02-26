using SWGen;

namespace CommandLine;

public class Post : ITitled, IAuthored, ICreatable<Post>, ISubDirectory
{
    public required string Title { get; init; }
    public required DateTime Published { get; init; }
    public string Author { get; init; } = "Anonymous";
    public string[] Tags { get; init; } = [];
    public string[] Categories { get; init; } = [];

    public static Post Create()
    {
        return new Post { Title = "NO TITLE!", Published = DateTime.UnixEpoch };
    }

    public string[] SubDirectory()
    {
        return [..Categories, Published.Year.ToString(), Published.Month.ToString(), Published.Day.ToString()];
    }
}

