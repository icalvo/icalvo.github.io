using CommandLine;
using Markdig;
using RazorLight;
using RazorLight.Razor;
using SWGen;
using SWGen.FileSystems;
using SWGen.Generators;
using SWGen.Razor;
using SWGen.Razor.Atom;

var markdownPipeline =
    new MarkdownPipelineBuilder()
    .EnableTrackTrivia()
    .UseMathematics()
    .Build();

var result = await StaticMainTool.Process(
    args,
    GetLoaders,
    GetGeneratorsConfig);

return result;

string TransformMarkdownTag(string content) =>
    StringManipulation.TransformBetween(content, "$markdown$", "$/markdown$", s => Markdown.ToHtml(s, markdownPipeline));

// Here we define the loaders that will be used to load the content of the site.
// Order is important. Each loader will be able to access metadata from the previous loaders.
static ILoader[] GetLoaders(IRazorLightEngine engine, RazorLightProject project, IFileSystem fs) =>
[
    new ObjectLoader<SiteInfo>((_, projectRoot) => GlobalSiteInfo.SiteInfo(projectRoot)),
    new RazorWithMetadataLoader<Page>("pages", recursive:true, engine, fs, project),
    new RazorWithMetadataLoader<MusicWork>("music/works", recursive:true, engine, fs, project),
    new RazorWithMetadataLoader<Page>("music", recursive:false, engine, fs, project),
    new RazorWithMetadataLoader<Post>("posts", recursive:true, engine, fs, project)
];

GeneratorConfig[] GetGeneratorsConfig(IRazorLightEngine engine, IFileSystem fs) =>
[
    // new ("sass.fsx", GeneratorTrigger.new OnFileExt(".scss"), f => f.Extension == "css"),
    new (new RazorGenerator<Page>(engine, fs, TransformMarkdownTag), IsPage),
    new (new RazorGenerator<Page>(engine, fs, TransformMarkdownTag), IsMusicPage),
    new (new RazorGenerator<MusicWork>(engine, fs, TransformMarkdownTag), IsMusicWork),
    new (new RazorGenerator<Post>(engine, fs, TransformMarkdownTag), IsPost),
    new (new IndexPageRazorGenerator(engine, fs, TransformMarkdownTag), IsFile("index.cshtml")),
    new (new AtomGenerator<IndexPage, Post>(engine, fs), IsFile("atom.cshtml")),
    new (new SiteMapGenerator<IndexPage, Post>(engine, fs), IsFile("sitemap.cshtml")),
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
        ext == ".wav" ||
        ext == ".wrk" ||
        ext == ".musicxml" ||
        ext == ".mscz" ||
        ext == ".mxl" ||
        inputFile.FileName.Contains(".cwp") ||
        inputFile.Parts.Contains(".git");
        
    return !fileShouldBeExcluded;
}