using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class GlobalLoader : ILoader
{
    public Task<SiteContents> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default)
    {
        siteContents.Replace(new SiteInfo
        {
            Title = "Ignacio Calvo Blog",
            Owner = new Author
            {
                Name = "Ignacio Calvo Martínez",
                Email = "ignacio.calvo@gmail.com"
            },
            Description = "Professional developer and amateur musician.",
            PostPageSize = 5,
            GoogleSiteVerification = "lYI_fzae0p3Hx_Ta5lkgLAztpNDFaWHb39crE9KBYjA",
            GoogleAnalytics = "UA-1520724-2",
            SocialLinks = new()
            {
                { "twitter", "https://twitter.com/ignaciocalvo" },
                { "github", "https://github.com/icalvo" },
                { "linkedin", "https://www.linkedin.com/in/ignaciocalvomartinez" },
                { "youtube", "https://www.youtube.com/icalvo" },
                { "instagram", "https://www.instagram.com/ignaciocalvo2" },
                { "facebook", "https://www.facebook.com/ignacio.calvomartinez" },
            },
            BaseUrl = "",
            Url = new Uri("https://ignaciocalvo.com"),
            ProjectRoot = projectRoot,
            DisqusShortName = "ignaciocalvo",
        });
        
        return Task.FromResult(siteContents);
    }
}