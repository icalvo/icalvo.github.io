namespace SWGen.FileSystems;

public class AbsolutePathEx : PathEx, IEquatable<AbsolutePathEx>
{
    public string Drive { get; }
    public bool IsRoot => Parts.Length == 0;

    public AbsolutePathEx(string drive, string[] parts) : base(LoadParts(parts))
    {
        Drive = drive;
    }

    public new static AbsolutePathEx Create(string rawPath) =>
        PathEx.Create(rawPath) as AbsolutePathEx
        ?? throw new ArgumentException("Path must be absolute", nameof(rawPath));

    private static string[] LoadParts(string[] parts) =>
        parts.Aggregate(
            Array.Empty<string>(),
            (acc, part) =>
            {
                return (acc, part) switch
                {
                    (_, "" or ".") => acc,
                    ([], "..") => acc,
                    ([..var preceding, _], "..") => preceding,
                    (_, _) => [..acc, part]
                };
            });

    public override AbsolutePathEx Parent => IsRoot ? this : new (Drive, Parts[..^1]);

    public virtual string Normalized(IRawFileSystem fs)
    {
        return fs.PathNormalizeFolderSeparator(this);
    }

    public virtual string Normalized(IFileSystem fs)
    {
        return fs.Path.NormalizeFolderSeparator(this);
    }

    public override string Normalized(char dirSeparator)
    {
        return $"{Drive}{dirSeparator}{string.Join(dirSeparator, Parts)}";
    }

    public override AbsolutePathEx Combine(RelativePathEx right) => new (Drive, Parts.Concat(right.Parts).ToArray());

    public bool IsChildOrSame(AbsolutePathEx parent) => RelativeTo(parent) != null;
    public bool IsParentOrSame(AbsolutePathEx child) => child.RelativeTo(this) != null;

    public RelativePathEx? RelativeTo(AbsolutePathEx root)
    {
        if (Drive != root.Drive)
        {
            return null;
        }

        if (Parts.Length < root.Parts.Length) return null;
        if (root.Zip(this).Count(x => x.First == x.Second) != root.Count()) return null;
        return new RelativePathEx(this.SkipWhile((p, i) => i < root.Count() && root.Parts[i] == p).ToArray());
    }

    public AbsolutePathEx ReplaceExtension(string extension)
    {
        return new AbsolutePathEx(Drive, [..Parts[..^1], FileNameWithoutExtension + extension]);
    }

    public static implicit operator AbsolutePathEx(string rawPath) => Create(rawPath);
    public static AbsolutePathEx operator /(AbsolutePathEx left, RelativePathEx right) => left.Combine(right);
    public static AbsolutePathEx operator /(AbsolutePathEx left, string right) => left.Combine(right);

    public bool Equals(AbsolutePathEx? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Drive == other.Drive;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AbsolutePathEx)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Drive);
    }

    public static bool operator ==(AbsolutePathEx? left, AbsolutePathEx? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AbsolutePathEx? left, AbsolutePathEx? right)
    {
        return !Equals(left, right);
    }
}