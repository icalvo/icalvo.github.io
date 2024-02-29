using CommandLine;
using SWGen;

var rootLogger = new ConsoleSwgLogger();
var razorEngineFactory = new RazorEngineFactory(rootLogger);
var result = await StaticMainTool.Process(
    args,
    SiteConfig.GetConfig(razorEngineFactory),
    SiteConfig.GetLoaders(razorEngineFactory),
    rootLogger);

foreach (var key in await razorEngineFactory.Project!.GetKnownKeysAsync())
{
    rootLogger.Log(LogLevel.Error, key);
}

return result;