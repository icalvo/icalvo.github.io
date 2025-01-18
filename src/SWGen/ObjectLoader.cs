using SWGen.FileSystems;

namespace SWGen;

public class ObjectLoader<T> : ILoader
{
    private readonly Func<SiteContents, AbsolutePathEx, T> _func;

    public ObjectLoader(Func<SiteContents, AbsolutePathEx, T> func)
    {
        _func = func;
    }

    public Task<LoaderResult> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default)
    {
        siteContents.Replace(_func(siteContents, projectRoot));
        
        return Task.FromResult(new LoaderResult(siteContents, Completed: true));
    }
}