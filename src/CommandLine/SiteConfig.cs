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
        new (new IndexPageGenerator(razorEngineFactory), new GeneratorTrigger.OnFilePredicate((_, t) => t.FileName == "index.cshtml")),
        new (new AtomGenerator(razorEngineFactory), new GeneratorTrigger.OnFilePredicate((_, t) => t.FileName == "atom.cshtml")),
        new (new SiteMapGenerator(razorEngineFactory), new GeneratorTrigger.OnFilePredicate((_, t) => t.FileName == "sitemap.cshtml")),
        new (new StaticFileGenerator(), new GeneratorTrigger.OnFilePredicate(IsStatic)),
    ];

    private static bool IsPost(AbsolutePathEx inputRoot, RelativePathEx inputFile)
    {
        if (!inputFile.Parts.Contains("posts")) return false;
        if (inputFile.FileName.StartsWith('_')) return false;
        var ext = inputFile.Extension;
        if (ext != ".cshtml") return false;
        return true;
    }

    private static bool IsPage(AbsolutePathEx inputRoot, RelativePathEx inputFile)
    {
        if (inputFile.Parts.ElementAtOrDefault(0) != "pages") return false;
        if (inputFile.FileName.StartsWith('_')) return false;
        var ext = inputFile.Extension;
        if (ext != ".cshtml") return false;
        return true;
    }

    private static bool IsMusicPage(AbsolutePathEx inputRoot, RelativePathEx inputFile)
    {
        if (inputFile.Parts.ElementAtOrDefault(^2) != "music") return false;
        if (inputFile.FileName.StartsWith('_')) return false;
        if (inputFile.Extension != ".cshtml") return false;
        return true;
    }

    private static bool IsMusicWork(AbsolutePathEx inputRoot, RelativePathEx inputFile)
    {
        if (inputFile.Parts.ElementAtOrDefault(^3) != "works") return false;
        if (inputFile.Parts.ElementAtOrDefault(^4) != "music") return false;
        if (inputFile.FileName.StartsWith('_')) return false;
        if (inputFile.Extension != ".cshtml") return false;
        return true;
    }
    
    private static bool IsStatic(AbsolutePathEx inputRoot, RelativePathEx inputFile)
    {
        var ext = inputFile.Extension;
        var fileShouldBeExcluded =
            ext == ".cshtml" ||
            ext == ".markdown" ||
            inputFile.Parts.Contains(".git");
            
        return !fileShouldBeExcluded;
    }
}