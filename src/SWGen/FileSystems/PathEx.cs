using System.Collections;
using System.IO;

namespace SWGen.FileSystems;

public abstract class PathEx : IEnumerable<string>, IEquatable<PathEx>
{
    protected PathEx(string[] parts)
    {
        Parts = parts;
    }

    public static PathEx Create(string rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            return new RelativePathEx([]);
        }

        var split = rawPath.Split('/', '\\');
        return IsRoot(split[0])
            ? new AbsolutePathEx(split[0].ToUpperInvariant(), split[1..])
            : new RelativePathEx(split);
    }

    private static bool IsRoot(string d)
    {
        return (d is [var l, ':'] && char.IsLetter(l)) || d == "";
    }

    public abstract PathEx? Parent { get; }

    public abstract string Normalized(char dirSeparator);
    public abstract PathEx Combine(RelativePathEx right);

    public IEnumerator<string> GetEnumerator()
    {
        return Parts.AsEnumerable().GetEnumerator();
    }

    public override string ToString()
    {
        return Normalized('/');
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Parts.GetEnumerator();
    }

    public string FileName => Parts[^1];
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(Parts[^1]);
    public PathEx Directory => Parent!;
    public string Extension => Path.GetExtension(Parts[^1]);
    public string[] Parts { get; init; }
    public static implicit operator PathEx(string rawPath) => Create(rawPath);
    public static PathEx operator /(PathEx left, RelativePathEx right) => left.Combine(right);
    public static PathEx operator /(PathEx left, string right) => left.Combine(right);

    public bool Equals(PathEx? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Parts.SequenceEqual(other.Parts);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PathEx)obj);
    }

    public override int GetHashCode()
    {
        var hashCodeBuilder = new HashCode();
        foreach (var part in Parts)
        {
            hashCodeBuilder.Add(part);
        }

        return hashCodeBuilder.ToHashCode();
    }

    public static bool operator ==(PathEx? left, PathEx? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PathEx? left, PathEx? right)
    {
        return !Equals(left, right);
    }
}