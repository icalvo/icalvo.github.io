using RazorLight;
using RazorLight.Razor;
using SWGen;

namespace CommandLine;

public class RazorEngineFactory : IRazorEngineFactory
{
    private static IRazorLightEngine? _engine;
    public RazorLightProject? Project { get; private set; }

    public IRazorLightEngine Create(string projectRoot)
    {
        Project ??= new ViewImportsFileSystemRazorProject(projectRoot);
        return _engine ??= new RazorLightEngineBuilder().UseProject(Project)
            .SetOperatingAssembly(typeof(Program).Assembly).UseMemoryCachingProvider().Build();
    }
}