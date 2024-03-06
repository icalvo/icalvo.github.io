using System.Runtime.CompilerServices;
using SWGen.FileSystems;

namespace Tests;

public class PathExtTests
{
    private class PathExTestData : TheoryData<PathEx?, string?, string[], string?>
    {
        public PathExTestData()
        {
            Add("", null, []);
            Add("/", "", []);
            Add("/..", "", []);
            Add("c:/dir1/system32/", "C:", ["dir1", "system32"]);
            Add("c:/dir2/system32", "C:", ["dir2", "system32"]);
            Add("c:/dir3//system32", "C:", ["dir3", "system32"]);
            Add("c:/dir4/./system32", "C:", ["dir4", "system32"]);
            Add(@"c:/dir5/.\system32", "C:", ["dir5", "system32"]);
            Add("c:/dir6/../", "C:", []);
        
            Add("dir7/system32", null, ["dir7", "system32"]);
            Add("../dir8/system32", null, ["..", "dir8", "system32"]);
            Add("../dir8/../system32", null, ["..", "system32"]);
            Add(@"dir9/..\..", null, [".."]);
        
            Add("C:", "C:", []);

            var root = AbsolutePathEx.Create("C:");
            Add(root / "dir20", "C:", ["dir20"]);
            Add(root / "dir21" / "dir22" / "..", "C:", ["dir21"]);
            Add(AbsolutePathEx.Create(@"C:\dir10\dir11\dir12").RelativeTo(@"C:\dir10\"), null, ["dir11", "dir12"]);            
            Add(AbsolutePathEx.Create(@"C:\dir10\").RelativeTo(@"C:\dir10\"), null, []);
            
            Add(RelativePathEx.Create("dir20/index.txt").ReplaceExtension(".html"), null, ["dir20", "index.html"]);
            Add(RelativePathEx.Create("index.txt").ReplaceExtension(".html"), null, ["index.html"]);
        }

        private void Add(string a, string? b, string[] c, [CallerArgumentExpression(nameof(a))] string? expr = null)
        {
            base.Add(a, b, c, expr);
        }
        private new void Add(PathEx? a, string? b, string[] c, [CallerArgumentExpression(nameof(a))] string? expr = null)
        {
            base.Add(a, b, c, expr);
        }
    }

    [Theory]
    [ClassData(typeof(PathExTestData))]
#pragma warning disable xUnit1026
    public void PathExTest(PathEx? path, string? drive, string[] parts, string? testCase)
#pragma warning restore xUnit1026
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