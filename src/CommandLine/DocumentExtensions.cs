using SWGen;

namespace CommandLine;

public static class DocumentExtensions
{
    public static Uri? LinkTo<T>(this IDocument document, Func<Document<T>, bool> filter) where T : class, ICreatable<T>
    {
        var doc = document.SiteContents.TryGetValues<Document<T>>().FirstOrDefault(filter);
        if (doc != null) return doc.OutputFile.Url();
        document.SiteContents.DocsWithPendingLinks.Add(document);
        return null;

    }
}