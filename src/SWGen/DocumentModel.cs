namespace SWGen;

public class DocumentModel<TMetadata> where TMetadata : class
{
    public string? Title { get; set; }
    public TMetadata? Metadata { get; set; }
}