﻿using RazorLight;
using RazorLight.Razor;
using SWGen.FileSystems;

namespace SWGen;

public class StaticMainTool
{
    public async Task<int> Process(
        string[] args,
        Func<IRazorLightEngine, IFileSystem, GeneratorConfig[]> config,
        Func<IRazorLightEngine, RazorLightProject, IFileSystem, ILoader[]> loaders,
        ISwgLogger logger)
    {
        switch (args)
        {
            case ["build"]:
            {
                var fs = new FileSystem(new LocalFileSystem());
                var cwd = AbsolutePathEx.Create(fs.Directory.GetCurrentDirectory());
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                var project = new ViewImportsFileSystemRazorProject(projectRoot.Normalized(fs), fs);
                var engine = new RazorLightEngineBuilder().UseProject(project)
                    .UseMemoryCachingProvider().Build();

                var applicationService = new ApplicationService(fs);
                await applicationService.Build(projectRoot, outputRoot, config(engine, fs), loaders(engine, project, fs), logger);
                break;
            }
            case ["watch"]:
            {
                var fs = new FileSystem(new LocalFileSystem());
                var cwd = AbsolutePathEx.Create(fs.Directory.GetCurrentDirectory());
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                var mapOrigin = cwd / "input" / "music" / "works";
                var mapDest = AbsolutePathEx.Create(@"D:\Dropbox\ignacio\music\Icm.Completo");
                
                var project = new ViewImportsFileSystemRazorProject(projectRoot.Normalized(fs), fs);
                var engine = new RazorLightEngineBuilder().UseProject(project)
                    .UseMemoryCachingProvider().Build();

                var applicationService = new ApplicationService(fs);
                await applicationService.Watch(projectRoot, outputRoot, config(engine, fs), loaders(engine, project, fs), logger);
                break;
            }
            default:
                throw new Exception("Unknown command");
        }

        return 0;
    }
}
