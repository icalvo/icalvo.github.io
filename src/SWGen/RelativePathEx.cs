﻿namespace SWGen;

public class RelativePathEx : PathEx
{
    public RelativePathEx(string[] parts) : base(LoadParts(parts))
    {
    }
    public static RelativePathEx Self() => Create("");
    public new static RelativePathEx Create(string rawPath) =>
        PathEx.Create(rawPath) as RelativePathEx
        ?? throw new ArgumentException("Path must be relative", nameof(rawPath));

    public override PathEx ToAbsolute() => Create(Environment.CurrentDirectory) / this;

    private static string[] LoadParts(string[] parts) =>
        parts.Aggregate(
            Array.Empty<string>(),
            (acc, part) =>
            {
                return (acc, part) switch
                {
                    (_, "" or ".") => acc,
                    ([], "..") => [".."],
                    ([.., ".."], "..") => [..acc, ".."],
                    ([..var preceding, _], "..") => preceding,
                    (_, _) => [..acc, part]
                };
            });

    public override bool IsAbsolute => false;
    public override RelativePathEx? Parent => Parts.Length == 0 ? null : new (Parts[..^1]);
    public bool IsSelf => Parts.Length == 0;

    public override string Normalized(char dirSeparator)
    {
        return $"{string.Join(dirSeparator, Parts)}";
    }

    public override RelativePathEx Combine(RelativePathEx right) => new(Parts.Concat(right.Parts).ToArray());

    public Uri Url() =>new("/" + Normalized('/'), UriKind.Relative);

    public static implicit operator RelativePathEx(string rawPath) => Create(rawPath);
    public static RelativePathEx operator /(RelativePathEx left, string right) => left.Combine(right);

}