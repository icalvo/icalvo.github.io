using SWGen;

namespace CommandLine;

public static class SiteConfig
{
    private static readonly RazorEngineFactory RazorEngineFactory = new();

    public static readonly ILoader[] GetLoaders =
    [
        new GlobalLoader(),
        new RazorWithMetadataLoader<Page>("pages", recursive:true, RazorEngineFactory),
        new RazorWithMetadataLoader<MusicWork>("music/works", recursive:true, RazorEngineFactory),
        new RazorWithMetadataLoader<Page>("music", recursive:false, RazorEngineFactory),
        new RazorWithMetadataLoader<Post>("posts", recursive:true, RazorEngineFactory)
    ];

    public static Config GetConfig => new (
    [
        // new ("sass.fsx", GeneratorTrigger.new OnFileExt(".scss"), f => f.Extension == "css"),
        new (new RazorGenerator<Page>(RazorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsPage)),
        new (new RazorGenerator<Page>(RazorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsMusicPage)),
        new (new RazorGenerator<MusicWork>(RazorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsMusicWork)),
        new (new RazorGenerator<Post>(RazorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsPost)),
        new (new IndexPageGenerator(RazorEngineFactory), new GeneratorTrigger.OnFile("index.cshtml")),
        new (new StaticFileGenerator(), new GeneratorTrigger.OnFilePredicate(IsStatic)),
    ]);

    private static bool IsPost(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (!page.Contains("posts")) return false;
        if (page.FileName.StartsWith('_')) return false;
        var ext = page.Extension;
        if (ext != ".cshtml") return false;
        return !page.Contains("_public");
    }

    private static bool IsPage(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (page.ElementAtOrDefault(0) != "pages") return false;
        if (page.FileName.StartsWith('_')) return false;
        var ext = page.Extension;
        if (ext != ".cshtml") return false;
        return !page.Contains("_public");
    }

    private static bool IsMusicPage(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (page.ElementAtOrDefault(^2) != "music") return false;
        if (page.FileName.StartsWith('_')) return false;
        var ext = page.Extension;
        if (ext != ".cshtml") return false;
        return !page.Contains("_public");
    }

    private static bool IsMusicWork(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (page.ElementAtOrDefault(^3) != "works") return false;
        if (page.ElementAtOrDefault(^4) != "music") return false;
        if (page.FileName.StartsWith('_')) return false;
        var ext = page.Extension;
        if (ext != ".cshtml") return false;
        return !page.Contains("_public");
    }
    
    private static bool IsStatic(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        var ext = page.Extension;
        var fileShouldBeExcluded =
            ext == ".cshtml" ||
            page.Contains("_public") ||
            page.Contains(".git");
            
        return !fileShouldBeExcluded;
    }
}