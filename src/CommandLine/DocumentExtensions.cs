using System.Runtime.CompilerServices;
using SWGen;

namespace CommandLine;

public static class DocumentExtensions
{
    public static Uri? LinkTo<T>(this IDocument document, Func<T, bool> filter, [CallerArgumentExpression(nameof(filter))] string? argumentName = null) where T : class, ICreatable<T>
    {
        var linkedDoc = document.SiteContents.TryGetValues<Document<T>>().FirstOrDefault(d => filter(d.Metadata));
        if (linkedDoc != null) return linkedDoc.OutputFile.Url();

        document.PendingLinks.Add($"{typeof(T).Name} where {argumentName ?? "???"}");
        return null;
    }
}