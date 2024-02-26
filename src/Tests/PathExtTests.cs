using SWGen;

namespace Tests;

public class PathExtTests
{
    public static IEnumerable<object[]> GetData()
    {
        yield return Case("", null, []);
        yield return Case("/", "", []);
        yield return Case("/..", "", [], "absdotdot");
        yield return Case("c:/dir1/system32/", "C:", ["dir1", "system32"]);
        yield return Case("c:/dir2/system32", "C:", ["dir2", "system32"]);
        yield return Case("c:/dir3//system32", "C:", ["dir3", "system32"]);
        yield return Case("c:/dir4/./system32", "C:", ["dir4", "system32"]);
        yield return Case(@"c:/dir5/.\system32", "C:", ["dir5", "system32"]);
        yield return Case("c:/dir6/../", "C:", []);
        
        yield return Case("dir7/system32", null, ["dir7", "system32"]);
        yield return Case("../dir8/system32", null, ["..", "dir8", "system32"]);
        yield return Case("../dir8/../system32", null, ["..", "system32"]);
        yield return Case(@"dir9/..\..", null, [".."]);

        
        yield return Case("C:", "C:", []);
        var root = AbsolutePathEx.Create("C:");
        yield return Case(root / "dir20", "C:", ["dir20"]);
        yield return Case(root / "dir21" / "dir22" / "..", "C:", ["dir21"]);
        yield return Case(AbsolutePathEx.Create(@"C:\dir10\dir11\dir12").RelativeTo(@"C:\dir10\"), null, ["dir11", "dir12"]);
    }

    private static object[] Case(PathEx? path, string? drive, string[] parts, string disc = "")
    {
        return [path, drive, parts, disc];
    }

    [Theory]
    [MemberData(nameof(GetData))]
    public void PathExTest(PathEx? path, string? drive, string[] parts, string disc = "")
    {
        Assert.NotNull(path);
        Assert.Equal(drive, (path as AbsolutePathEx)?.Drive);
        Assert.Equal(parts, path.Parts);
    }

    [Fact]
    public void ConstructorFailures()
    {
        Assert.Throws<ArgumentException>(() => AbsolutePathEx.Create(@"dir10\dir11\dir12"));
        Assert.Throws<ArgumentException>(() => RelativePathEx.Create(@"C:\dir10\dir11\dir12"));
    }
    [Fact]
    public void RelativeTo_NonRelatives()
    {
        Assert.Null(AbsolutePathEx.Create(@"C:\dir10\dir11\dir12").RelativeTo(@"C:\dir10\dir13"));
    }
}