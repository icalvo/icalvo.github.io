using RazorLight;
using RazorLight.Razor;
using SWGen;

namespace CommandLine;

public class RazorEngineFactory : IRazorEngineFactory
{
    private static IRazorLightEngine? _engine;
    private readonly ISwgLogger _logger;

    public RazorEngineFactory(ISwgLogger logger)
    {
        _logger = logger;
    }

    public RazorLightProject? Project { get; private set; }

    public IRazorLightEngine Create(string projectRoot)
    {
        Project ??= new ViewImportsFileSystemRazorProject(projectRoot, _logger);
        return _engine ??= new RazorLightEngineBuilder().UseProject(Project)
            .SetOperatingAssembly(typeof(Program).Assembly).UseMemoryCachingProvider().Build();
    }
}