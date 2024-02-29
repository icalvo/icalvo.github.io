using SWGen;

namespace CommandLine;

public static class SiteConfig
{
    // Here we define the loaders that will be used to load the content of the site.
    // Order is important. Each loader will be able to access metadata from the previous loaders.
    public static ILoader[] GetLoaders(IRazorEngineFactory razorEngineFactory) =>
    [
        new GlobalLoader(),
        new RazorWithMetadataLoader<Page>("pages", recursive:true, razorEngineFactory),
        new RazorWithMetadataLoader<MusicWork>("music/works", recursive:true, razorEngineFactory),
        new RazorWithMetadataLoader<Page>("music", recursive:false, razorEngineFactory),
        new RazorWithMetadataLoader<Post>("posts", recursive:true, razorEngineFactory)
    ];

    public static GeneratorConfig[] GetConfig(IRazorEngineFactory razorEngineFactory) =>
    [
        // new ("sass.fsx", GeneratorTrigger.new OnFileExt(".scss"), f => f.Extension == "css"),
        new (new RazorGenerator<Page>(razorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsPage)),
        new (new RazorGenerator<Page>(razorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsMusicPage)),
        new (new RazorGenerator<MusicWork>(razorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsMusicWork)),
        new (new RazorGenerator<Post>(razorEngineFactory), new GeneratorTrigger.OnFilePredicate(IsPost)),
        new (new IndexPageGenerator(razorEngineFactory), new GeneratorTrigger.OnFile("index.cshtml")),
        new (new AtomGenerator(razorEngineFactory), new GeneratorTrigger.OnFile("atom.cshtml")),
        new (new SiteMapGenerator(razorEngineFactory), new GeneratorTrigger.OnFile("sitemap.cshtml")),
        new (new StaticFileGenerator(), new GeneratorTrigger.OnFilePredicate(IsStatic)),
    ];

    private static bool IsPost(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (!page.Parts.Contains("posts")) return false;
        if (page.FileName.StartsWith('_')) return false;
        var ext = page.Extension;
        if (ext != ".cshtml") return false;
        return true;
    }

    private static bool IsPage(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (page.Parts.ElementAtOrDefault(0) != "pages") return false;
        if (page.FileName.StartsWith('_')) return false;
        var ext = page.Extension;
        if (ext != ".cshtml") return false;
        return true;
    }

    private static bool IsMusicPage(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (page.Parts.ElementAtOrDefault(^2) != "music") return false;
        if (page.FileName.StartsWith('_')) return false;
        if (page.Extension != ".cshtml") return false;
        return true;
    }

    private static bool IsMusicWork(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        if (page.Parts.ElementAtOrDefault(^3) != "works") return false;
        if (page.Parts.ElementAtOrDefault(^4) != "music") return false;
        if (page.FileName.StartsWith('_')) return false;
        if (page.Extension != ".cshtml") return false;
        return true;
    }
    
    private static bool IsStatic(AbsolutePathEx projectRoot, RelativePathEx page)
    {
        var ext = page.Extension;
        var fileShouldBeExcluded =
            ext == ".cshtml" ||
            ext == ".markdown" ||
            page.Parts.Contains(".git");
            
        return !fileShouldBeExcluded;
    }
}