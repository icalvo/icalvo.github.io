using CommandLine;
using SWGen;
using SWGen.FileSystems;

var rootLogger = new ConsoleSwgLogger(enableDebug: false);
var fs = new FileSystem(new LocalFileSystem());
var razorEngineFactory = new RazorEngineFactory(fs);

var smt = new StaticMainTool(fs, new ApplicationService(fs));
var result = await smt.Process(
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
    new RazorWithMetadataLoader<Page>("pages", recursive:true, razorEngineFactory, fs),
    new RazorWithMetadataLoader<MusicWork>("music/works", recursive:true, razorEngineFactory, fs),
    new RazorWithMetadataLoader<Page>("music", recursive:false, razorEngineFactory, fs),
    new RazorWithMetadataLoader<Post>("posts", recursive:true, razorEngineFactory, fs)
];

GeneratorConfig[] GetConfig() =>
[
    // new ("sass.fsx", GeneratorTrigger.new OnFileExt(".scss"), f => f.Extension == "css"),
    new (new RazorGenerator<Page>(razorEngineFactory, fs), IsPage),
    new (new RazorGenerator<Page>(razorEngineFactory, fs), IsMusicPage),
    new (new RazorGenerator<MusicWork>(razorEngineFactory, fs), IsMusicWork),
    new (new RazorGenerator<Post>(razorEngineFactory, fs), IsPost),
    new (new IndexPageGenerator(razorEngineFactory, fs), IsFile("index.cshtml")),
    new (new AtomGenerator(razorEngineFactory, fs), IsFile("atom.cshtml")),
    new (new SiteMapGenerator(razorEngineFactory, fs), IsFile("sitemap.cshtml")),
    new (new StaticFileGenerator(fs), IsStatic),
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