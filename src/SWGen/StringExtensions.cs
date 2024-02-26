namespace SWGen;

public static class StringExtensions
{
    public static string CutFromLine(this string content, string lineStart) =>
        content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .TakeWhile(line => !line.StartsWith(lineStart)).StringJoin(Environment.NewLine);
    
    public static string? ReplaceEnd(this string text, string ending, string newEnding) =>
        text.EndsWith(ending) ? text[..^ending.Length] + newEnding : null;
}