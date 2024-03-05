using RazorLight;
using RazorLight.Razor;
using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class RazorEngineFactory : IRazorEngineFactory
{
    private static IRazorLightEngine? _engine;
    private readonly IFileSystem _fs;

    public RazorEngineFactory(IFileSystem fs)
    {
        _fs = fs;
    }

    public RazorLightProject? Project { get; private set; }

    public IRazorLightEngine Create(string projectRoot)
    {
        Project ??= new ViewImportsFileSystemRazorProject(projectRoot, _fs);
        return _engine ??= new RazorLightEngineBuilder().UseProject(Project)
            .SetOperatingAssembly(typeof(Program).Assembly).UseMemoryCachingProvider().Build();
    }
}