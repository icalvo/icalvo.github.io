using SWGen.FileSystems;

namespace SWGen;

public class ObjectLoader<T>(Func<SiteContents, AbsolutePathEx, T> func) : ILoader
{
    public Task<LoaderResult> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default)
    {
        siteContents.Replace(func(siteContents, projectRoot));

        return Task.FromResult(new LoaderResult(siteContents, Completed: true));
    }
}
