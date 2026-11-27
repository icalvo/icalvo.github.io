using System.Globalization;
using Slugify;
using SWGen;
using SWGen.FileSystems;
using SWGen.Razor;
using SWGen.Razor.Atom;

namespace CommandLine;

public class Post : ITitled, IAuthored, ICreatable<Post>, ILink, IIdentifyable<PostIdentifier>, IDated
{
    public PostIdentifier? Id { get; init; }
    public required string Title { get; init; }
    public required DateTime Published { get; init; }
    public string Author { get; init; } = "Anonymous";
    public string[] Tags { get; init; } = [];
    public string[] Categories { get; init; } = [];


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
            Published.Year.ToString(CultureInfo.InvariantCulture),
            Published.Month.ToString(CultureInfo.InvariantCulture),
            Published.Day.ToString(CultureInfo.InvariantCulture),
            $"{_slugHelper.GenerateSlug(Title)}.html"
        ]);
}
