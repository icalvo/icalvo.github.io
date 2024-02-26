using RazorLight.Razor;

namespace SWGen;

public static class RazorLightProjectExtensions
{
    public static void RegisterGenerationContent(this RazorLightProject project, string key, string content)
    {
        if (project is ViewImportsFileSystemRazorProject p)
        {
            p.RegisterContent(project.NormalizeKey(key), content);
        }
    }
}