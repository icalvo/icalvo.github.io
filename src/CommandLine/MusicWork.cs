using SWGen;

namespace CommandLine;

public class MusicWork : ITitled, IAuthored, ICreatable<MusicWork>
{
    public MusicWorkIdentifier? Id { get; init; }
    public required string Title { get; init; }
    public required PartialDate CompositionDate { get; init; }
    public string Author => "Ignacio Calvo";
    public required int CatalogueNumber { get; init; }
    public string? CatalogueLetter { get; init; }
    public Movement[] Movements { get; init; } = [];
    public string? DefaultAudioMovementKey { get; init; }
    public string[] Instrumentation { get; init; } = [];

    public string CompleteTitle => $"{Title}, Cat.&nbsp;{CatalogueNumber}{CatalogueLetter}";
    public static MusicWork Create()
    {
        return new MusicWork { Title = "NO TITLE!", CompositionDate = new PartialDate(1970), CatalogueNumber = 0 };
    }
}
