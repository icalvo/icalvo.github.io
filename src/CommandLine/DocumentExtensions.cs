using SWGen;
using static CommandLine.PostIdentifier;

namespace CommandLine;

public static class DocumentExtensions
{
    public static Uri? LinkTo<T>(this IDocument document, Func<T, bool> filter) where T : class, ICreatable<T>
    {
        var doc = document.SiteContents.TryGetValues<Document<T>>().FirstOrDefault(d => filter(d.Metadata));
        if (doc != null) return doc.OutputFile.Url();
        document.SiteContents.DocsWithPendingLinks.Add(document);
        return null;
    }
}