namespace SWGen;

public interface ILoader
{
    Task<SiteContents> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default);
}