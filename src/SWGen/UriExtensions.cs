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