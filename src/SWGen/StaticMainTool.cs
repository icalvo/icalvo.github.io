using SWGen.FileSystems;

namespace SWGen;

public class StaticMainTool
{
    private readonly IFileSystem _fs;
    private readonly ApplicationService _applicationService;

    public StaticMainTool(IFileSystem fs, ApplicationService applicationService)
    {
        _fs = fs;
        _applicationService = applicationService;
    }

    public async Task<int> Process(string[] args, GeneratorConfig[] config, ILoader[] loaders, ISwgLogger logger)
    {
        var cwd = AbsolutePathEx.Create(_fs.Directory.GetCurrentDirectory());
        switch (args)
        {
            case ["build"]:
            {
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                await _applicationService.Build(projectRoot, outputRoot, config, loaders, logger);
                break;
            }
            case ["watch"]:
            {
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                await _applicationService.Watch(projectRoot, outputRoot, config, loaders, logger);
                break;
            }
            default:
                throw new Exception("Unknown command");
        }

        return 0;
    }
}
