using CommandLine;
using SWGen;

return await StaticMainTool.Process(
    args,
    SiteConfig.GetConfig,
    SiteConfig.GetLoaders);

