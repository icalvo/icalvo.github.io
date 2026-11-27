using System.Collections.Generic;

namespace FileSystemSourceGenerator;

public static class CharExtensions
{
    public static string StrJoin<T>(this IEnumerable<T> source, string separator) =>
        string.Join(separator, source);

    public static bool IsDigit(this char c) => c is >= '0' and <= '9';
}
