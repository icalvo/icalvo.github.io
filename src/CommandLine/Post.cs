using Slugify;
using SWGen;

namespace CommandLine;

public class Post : ITitled, IAuthored, ICreatable<Post>, ILink
{
    public required string Title { get; init; }
    public required DateTime Published { get; init; }
    public string Author { get; init; } = "Anonymous";
    public string[] Tags { get; init; } = [];
    public string[] Categories { get; init; } = [];
    public PostIdentifiers? Id { get; init; } = null;

    public static Post Create()
    {
        return new Post { Title = "NO TITLE!", Published = DateTime.UnixEpoch };
    }

    private readonly SlugHelper _slugHelper = new();

    public RelativePathEx BuildLink(IDocument doc) =>
        new(
        [
            "posts",
            ..Categories,
            Published.Year.ToString(),
            Published.Month.ToString(),
            Published.Day.ToString(),
            $"{_slugHelper.GenerateSlug(Title)}.html"
        ]);
}