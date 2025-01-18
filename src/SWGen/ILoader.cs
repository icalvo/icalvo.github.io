using SWGen.FileSystems;

namespace SWGen;

public interface ILoader
{
    Task<LoaderResult> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default);
}