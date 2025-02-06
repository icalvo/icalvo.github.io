using RazorLight;
using RazorLight.Razor;
using SWGen;
using SWGen.FileSystems;
using SWGen.Generators;
using SWGen.Razor;

namespace CommandLine;

public static class StaticMainTool
{
    public static async Task<int> Process(string[] args,
        Func<IRazorLightEngine, RazorLightProject, IFileSystem, ILoader[]> getLoaders,
        Func<IRazorLightEngine, IFileSystem, GeneratorConfig[]> getGeneratorConfigs,
        ISwgLogger logger)
    {
        var localFileSystem = new LocalFileSystem();
        switch (args.FirstOrDefault())
        {
            case "build":
            {
                var fs = new FileSystem(localFileSystem);
                var cwd = AbsolutePathEx.Create(fs.Directory.GetCurrentDirectory());
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                var project = new ViewImportsFileSystemRazorProject(projectRoot.Normalized(fs), fs);
                var engine = new RazorLightEngineBuilder().UseProject(project)
                    .UseMemoryCachingProvider().Build();

                var applicationService = new ApplicationService(fs);
                var loaders = getLoaders(engine, project, fs);
                var generatorConfigs = getGeneratorConfigs(engine, fs);
                await applicationService.Build(projectRoot, outputRoot, generatorConfigs, loaders, logger);
                break;
            }
            case "watch":
            {
                var fs = new FileSystem(localFileSystem);
                var cwd = AbsolutePathEx.Create(fs.Directory.GetCurrentDirectory());
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                
                var project = new ViewImportsFileSystemRazorProject(projectRoot.Normalized(fs), fs);
                var engine = new RazorLightEngineBuilder().UseProject(project)
                    .UseMemoryCachingProvider().Build();

                var applicationService = new ApplicationService(fs);
                var loaders = getLoaders(engine, project, fs);
                var generatorConfigs = getGeneratorConfigs(engine, fs);
                await applicationService.Watch(projectRoot, outputRoot, generatorConfigs, loaders, logger);
                break;
            }
            default:
                throw new Exception("Unknown command");
        }

        return 0;
    }
}
