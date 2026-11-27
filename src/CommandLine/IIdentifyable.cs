namespace CommandLine;

public interface IIdentifyable<TIdentifier> where TIdentifier : struct
{
    TIdentifier? Id { get; }
}
