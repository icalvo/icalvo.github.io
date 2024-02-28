using System.Collections;

namespace SWGen;

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


    public abstract PathEx ToAbsolute();

    public abstract bool IsAbsolute { get; }

    public abstract PathEx? Parent { get; }

    public string Normalized() => Normalized(Path.DirectorySeparatorChar);
    public abstract string Normalized(char dirSeparator);
    public abstract PathEx Combine(RelativePathEx right);

    public IEnumerator<string> GetEnumerator()
    {
        return Parts.AsEnumerable().GetEnumerator();
    }

    public override string ToString()
    {
        return Normalized();
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
    public static explicit operator string(PathEx path) => path.Normalized();
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
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PathEx)obj);
    }

    public override int GetHashCode()
    {
        return Parts.GetHashCode();
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