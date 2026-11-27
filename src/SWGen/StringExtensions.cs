namespace SWGen;

public static class StringExtensions
{
    public static string UntilLine(this string content, string lineStart) =>
        content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .TakeWhile(line => !line.StartsWith(lineStart, StringComparison.Ordinal)).StringJoin(Environment.NewLine);

    public static string? ReplaceEnd(this string text, string ending, string newEnding) =>
        text.EndsWith(ending, StringComparison.Ordinal) ? text[..^ending.Length] + newEnding : null;
}
