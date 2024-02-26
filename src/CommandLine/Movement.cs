namespace CommandLine;

public record Movement
{
    public required string Name { get; init; }
    public required string Key { get; init; }
    public required string Description { get; init; }
}