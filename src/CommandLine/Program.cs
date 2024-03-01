using CommandLine;
using SWGen;

var rootLogger = new ConsoleSwgLogger();
var razorEngineFactory = new RazorEngineFactory(rootLogger);
var result = await StaticMainTool.Process(
    args,
    GetConfig(),
    GetLoaders(),
    rootLogger);

return result;

// Here we define the loaders that will be used to load the content of the site.
// Order is important. Each loader will be able to access metadata from the previous loaders.
ILoader[] GetLoaders() =>
[
    new GlobalLoader(),
    new RazorWithMetadataLoader<Page>("pages", recursive:true, razorEngineFactory),
    new RazorWithMetadataLoader<MusicWork>("music/works", recursive:true, razorEngineFactory),
    new RazorWithMetadataLoader<Page>("music", recursive:false, razorEngineFactory),
    new RazorWithMetadataLoader<Post>("posts", recursive:true, razorEngineFactory)
];

GeneratorConfig[] GetConfig() =>
[
    // new ("sass.fsx", GeneratorTrigger.new OnFileExt(".scss"), f => f.Extension == "css"),
    new (new RazorGenerator<Page>(razorEngineFactory), IsPage),
    new (new RazorGenerator<Page>(razorEngineFactory), IsMusicPage),
    new (new RazorGenerator<MusicWork>(razorEngineFactory), IsMusicWork),
    new (new RazorGenerator<Post>(razorEngineFactory), IsPost),
    new (new IndexPageGenerator(razorEngineFactory), IsFile("index.cshtml")),
    new (new AtomGenerator(razorEngineFactory), IsFile("atom.cshtml")),
    new (new SiteMapGenerator(razorEngineFactory), IsFile("sitemap.cshtml")),
    new (new StaticFileGenerator(), IsStatic),
];

static Func<AbsolutePathEx, RelativePathEx, bool> IsFile(string fileName)
{
    return (_, t) => t.FileName == fileName;
}
static bool IsPost(AbsolutePathEx inputRoot, RelativePathEx inputFile)
{
    if (!inputFile.Parts.Contains("posts")) return false;
    if (inputFile.FileName.StartsWith('_')) return false;
    var ext = inputFile.Extension;
    if (ext != ".cshtml") return false;
    return true;
}

static bool IsPage(AbsolutePathEx inputRoot, RelativePathEx inputFile)
{
    if (inputFile.Parts.ElementAtOrDefault(0) != "pages") return false;
    if (inputFile.FileName.StartsWith('_')) return false;
    var ext = inputFile.Extension;
    if (ext != ".cshtml") return false;
    return true;
}

static bool IsMusicPage(AbsolutePathEx inputRoot, RelativePathEx inputFile)
{
    if (inputFile.Parts.ElementAtOrDefault(^2) != "music") return false;
    if (inputFile.FileName.StartsWith('_')) return false;
    if (inputFile.Extension != ".cshtml") return false;
    return true;
}

static bool IsMusicWork(AbsolutePathEx inputRoot, RelativePathEx inputFile)
{
    if (inputFile.Parts.ElementAtOrDefault(^3) != "works") return false;
    if (inputFile.Parts.ElementAtOrDefault(^4) != "music") return false;
    if (inputFile.FileName.StartsWith('_')) return false;
    if (inputFile.Extension != ".cshtml") return false;
    return true;
}

static bool IsStatic(AbsolutePathEx inputRoot, RelativePathEx inputFile)
{
    var ext = inputFile.Extension;
    var fileShouldBeExcluded =
        ext == ".cshtml" ||
        ext == ".markdown" ||
        ext == ".dorico" ||
        ext == ".sib" ||
        ext == ".cwp" ||
        ext == ".mus" ||
        inputFile.FileName.Contains(".cwp") ||
        inputFile.Parts.Contains(".git");
        
    return !fileShouldBeExcluded;
}