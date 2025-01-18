using System.IO;
using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace SWGen;

public static class UriExtensions
{
    public static Uri Combine(this Uri baseUri, string path, params string[] paths) =>
        string.IsNullOrWhiteSpace(path)
            ? baseUri
            : paths.Aggregate(
                new Uri(baseUri, path),
                (current, extendedPath) => new Uri(current, extendedPath));
}

public static class TidyExtensions
{
    public static string Tidy(this string untidy)
    {
        var parser = new HtmlParser();
        var document = parser.ParseDocument(untidy);
        var sw = new StringWriter();
        document.ToHtml(sw, new PrettyMarkupFormatter());
        return sw.ToString();
    }
}
