using System.Text.RegularExpressions;

namespace CommandLine;

public static class StringManipulation
{
    private static readonly Dictionary<string, Regex> TagRegexDictionary = new();
    public static string TransformChunk(string content, string beginPattern, string endPattern, Func<string, string> transform)
    {
        Regex regex =
            TagRegexDictionary.TryGetValue(beginPattern, out var found)
                ? found
                : TagRegexDictionary[beginPattern] = new Regex($"{beginPattern}(.*?){endPattern}", RegexOptions.Singleline | RegexOptions.Compiled);

        return regex.Replace(
            content,
            m => transform(m.Groups[1].Value));
    }

    public static string TransformBetween(string content, string begin, string end, Func<string, string> transform)
    {
        var beginEsc = Regex.Escape(begin);
        var endEsc = Regex.Escape(end);
        return TransformChunk(content, beginEsc, endEsc, transform);
    }

    public static string TransformTags(string content, string tag, Func<string, string> transform)
    {
        var tagEsc = Regex.Escape(tag);
        return TransformChunk(content, $"<{tagEsc}>", $@"</\w*{tagEsc}>", transform);
    }
    
    public static string CDataEscape(this string content) => content;
    
    public static string ExpandUrls(this string content, string root) => content;
}