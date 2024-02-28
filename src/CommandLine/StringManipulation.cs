using System.Text.RegularExpressions;

namespace CommandLine;

public static class StringManipulation
{
    private static readonly Dictionary<string, Regex> TagRegexDictionary = new();
    public static string TransformTags(string content, string tag, Func<string, string> transform)
    {
        Regex regex =
            TagRegexDictionary.TryGetValue(tag, out var found)
                ? found
                : TagRegexDictionary[tag] = new Regex($"<{tag}>(.*?)</{tag}>", RegexOptions.Singleline | RegexOptions.Compiled);

        return regex.Replace(
            content,
            m => transform(m.Groups[1].Value));
    }
}