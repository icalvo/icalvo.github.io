using RazorLight;
using RazorLight.Razor;

namespace CommandLine;

public interface IRazorEngineFactory
{
    IRazorLightEngine Create(string projectRoot);
    RazorLightProject? Project { get; }
}