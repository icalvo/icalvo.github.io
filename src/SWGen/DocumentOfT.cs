using SWGen.FileSystems;

namespace SWGen;

public class Document<TMetadata>(SiteContents siteContents, RelativePathEx file, IFileSystem fs)
    : Document(siteContents, file, fs) where TMetadata : class, ICreatable<TMetadata>
{
    public TMetadata Metadata
    {
        get
        {
            if (This.Metadata is TMetadata typed)
            {
                return typed;
            }

            var created = TMetadata.Create();
            This.Metadata = created;
            return created;
        }
        set => This.Metadata = value;
    }
}
