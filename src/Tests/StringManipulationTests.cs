using CommandLine;

namespace Tests;

public class StringManipulationTests
{
    [Fact]
    public void Test1()
    {
        Assert.Equal(
            """
            <html>
            <body>
              Hello world THIS IS IMPORTANT an also THIS.
            </body>
            </html>
            """,
            StringManipulation.TransformTags(
                """
                <html>
                <body>
                  Hello world <upper>this is important</upper> an also <upper>this</upper>.
                </body>
                </html>
                """,
                "upper",
                s => s.ToUpperInvariant())
        );
    }
    
    [Fact]
    public void Test2()
    {
        Assert.Equal("\r\n",
            StringManipulation.TransformTags("<markdown>\r\n</markdown>", "markdown", s => s.ToUpperInvariant())
        );
    }
}