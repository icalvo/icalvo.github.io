using System;
using System.Collections.Generic;

namespace SWGen.Razor;

public static class EnumerableHelpers
{
    public static IEnumerable<T> Unfold<T>(this T seed, Func<T, bool> isLast, Func<T, T> generator)
    {
        var current = seed;
        while (true)
        {
            yield return current;
            if (isLast(current)) yield break;
            current = generator(current);
        }
    }
}
