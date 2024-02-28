namespace SWGen;

public interface ILoader
{
    Task<SiteContents> Load(SiteContents siteContent, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default);
}