using CommandLine;
using SWGen;

var rootLogger = new ConsoleSwgLogger();
var razorEngineFactory = new RazorEngineFactory(rootLogger);
return await StaticMainTool.Process(
    args,
    SiteConfig.GetConfig(razorEngineFactory),
    SiteConfig.GetLoaders(razorEngineFactory),
    rootLogger);

