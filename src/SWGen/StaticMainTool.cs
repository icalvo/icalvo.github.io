using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;

namespace SWGen;

public static class StaticMainTool
{

    public static async Task<int> Process(string[] args, GeneratorConfig[] config, ILoader[] loaders, ISwgLogger logger)
    {
        var cwd = AbsolutePathEx.Create(Directory.GetCurrentDirectory());
        switch (args)
        {
            case ["build"]:
            {
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                await GuardedGenerate(config, loaders, projectRoot, outputRoot, logger, CancellationToken.None);

                break;
            }
            case ["watch"]:
            {
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                await Watch(
                    config,
                    loaders,
                    projectRoot,
                    new ClientWebSocket(),
                    outputRoot,
                    logger,
                    CancellationToken.None);
                break;
            }
            default:
                throw new Exception("Unknown command");
        }

        return 0;
    }

    private static async Task GuardedGenerate(GeneratorConfig[] config, ILoader[] loaders, AbsolutePathEx projectRoot,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        logger.Info($"Input: {projectRoot}");
        logger.Info($"Output: {outputRoot}");
        try
        {
            await GenerateFolder(projectRoot, isWatch: false, config, loaders, outputRoot, logger, ct);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToStringDemystified());
        }
    }

    public static async Task Watch(GeneratorConfig[] cfg, ILoader[] loaders, AbsolutePathEx projectRoot, WebSocket webSocket,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        var lastAccessed = new Dictionary<string, DateTime>();
        await GuardedGenerate(cfg, loaders, Directory.GetCurrentDirectory(), outputRoot, logger, ct);

        using var watcher = new FileSystemWatcher();
        watcher.Path = projectRoot.Normalized();
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.FileName;

        watcher.Created += Handler;
        watcher.Changed += Handler;
        watcher.Deleted += Handler;


        watcher.Created += ContentChangedHandler;
        watcher.Changed += ContentChangedHandler;
        watcher.Deleted += ContentChangedHandler;

        SimpleWebServer(outputRoot, logger);

        return;

        void Handler(object sender, FileSystemEventArgs args)
        {
            var pathDirectories = AbsolutePathEx.Create(args.FullPath).RelativeTo(projectRoot) ??
                                  throw new Exception("Should not happen");
            var shouldHandle = !pathDirectories.Parts.Any(
                fragment => fragment is "_public" or ".git" or "bin" or "obj" or ".sass-cache" or ".ionide");
            if (shouldHandle)
            {
                var lastTimeWrite = File.GetLastWriteTime(args.FullPath);
                if (!lastAccessed.TryGetValue(args.FullPath, out var lastTimeAccessed) ||
                    Math.Abs((lastTimeAccessed - lastTimeWrite).Seconds) >= 1)
                {
                    logger.Info($"Changes detected in {args.FullPath}, regenerating...");
                    lastAccessed[args.FullPath] = lastTimeWrite;
                    GuardedGenerate(cfg, loaders, Directory.GetCurrentDirectory(), outputRoot, logger, ct).Wait(ct);
                }
            }
        }

        void ContentChangedHandler(object sender, FileSystemEventArgs args)
        {
            webSocket.SendAsync(
                ArraySegment<byte>.Empty,
                WebSocketMessageType.Close,
                endOfMessage: true,
                CancellationToken.None).Wait(ct);
        }

    }

    public static void SimpleWebServer(AbsolutePathEx localPath, ISwgLogger logger, params string[] prefixes)
    {
        if (!HttpListener.IsSupported)
        {
            logger.Error("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
            return;
        }

        if (prefixes.Length == 0)
        {
            throw new ArgumentException("Prefixes are required, e.g. http://contoso.com:8080/index/");
        }

        // Create a listener.
        HttpListener listener = new HttpListener();
        // Add the prefixes.
        foreach (string s in prefixes)
        {
            listener.Prefixes.Add(s);
        }

        listener.Start();
        logger.Info($"Listening to {string.Join(", ", prefixes)}...");
        while (true)
        {

            // Note: The GetContext method blocks while waiting for a request.
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.
            if (request.Url!.LocalPath == "/websocket")
            {
                return;
            }

            var file = localPath / request.Url!.LocalPath;
            if (File.Exists(file.Normalized()))
            {
                using var input = File.OpenRead(file.Normalized());
                response.ContentLength64 = input.Length;
                response.StatusCode = 200;
                using var output = response.OutputStream;
                input.CopyTo(output);
                input.Close();
                output.Close();
            }
            else
            {
                response.StatusCode = 404;
            }
        }
    }

    public static async Task GenerateFolder(AbsolutePathEx projectRoot, bool isWatch, GeneratorConfig[] config, ILoader[] loaders,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var sc = new SiteContents();

        var loadLogger1 = logger.BeginScope("Load (1st pass)");
        foreach (var loader in loaders)
        {
            var loaderLogger = loadLogger1.BeginScope(loader.ToString() ?? "");
            sc = await loader.Load(sc, projectRoot, loaderLogger, ct);
        }

        var loadLogger2 = logger.BeginScope("Load (2nd pass)");
        foreach (var loader in loaders)
        {
            var loaderLogger = loadLogger2.BeginScope(loader.ToString() ?? "");
            sc = await loader.Load(sc, projectRoot, loaderLogger, ct);
        }

        foreach (var error in sc.Errors())
        {
            logger.Error($"{error.Path}: {error.Message}");
        }

        var generateLogger = logger.BeginScope("Generation");
        var onceLogger = generateLogger.BeginScope("Once");
        {
            await RunOnceGenerators(config, sc, projectRoot, outputRoot, onceLogger, ct);
        }

        var fileBasedLogger = generateLogger.BeginScope("FileBased");
        await Parallel.ForEachAsync(
            Directory.GetFiles(projectRoot.Normalized(), "*", new EnumerationOptions { RecurseSubdirectories = true })
                .Select(AbsolutePathEx.Create),
            ct,
            async (filePath, token) =>
            {
                var relative = filePath.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
                var fileGeneratorLogger = fileBasedLogger.BeginScope(relative.Normalized());
                if (relative.Parts[0] is "bin" or "obj") return;
                if (sc.TryGetError(relative.Normalized()) != null) return;

                await Generate(config, sc, projectRoot, relative.Normalized(), outputRoot, fileGeneratorLogger, token);
            });

        logger.Info($"Overall time: {sw.Elapsed}");
    }

    private static async Task RunOnceGenerators(GeneratorConfig[] cfg, SiteContents siteContent, AbsolutePathEx projectRoot,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        foreach (var n in cfg.Where(n => n.Trigger is GeneratorTrigger.Once).Select(n => n.Generator))
        {
            await GenerateAux(n, siteContent, projectRoot, outputRoot, logger, ct);
        }
    }

    private static async Task Generate(GeneratorConfig[] cfg, SiteContents siteContents, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        siteContents.ContentAvailable = true;
        GeneratorConfig? pick = cfg.FirstOrDefault(n => n.MatchesFile(projectRoot, inputFile));
        if (pick == null)
        {
            logger.Debug("Ignored");
            return;
        }

        logger.Debug($"Picked [{pick}]");

        await GenerateAux(pick.Generator, siteContents, projectRoot, outputRoot, logger, ct, inputFile);
    }

    private static async Task GenerateAux(Generator generator, SiteContents siteContents, AbsolutePathEx projectRoot,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct, RelativePathEx? inputFile = null)
    {
        var tempDir = Path.GetTempPath();
        inputFile ??= RelativePathEx.Self();
        var sw = Stopwatch.StartNew();
        var results = generator.Generate(siteContents, projectRoot, inputFile, logger, ct);
        List<AbsolutePathEx> outputPaths = new();
        foreach (var result in results)
        {
            var resultLogger = logger.BeginScope(result.File.Normalized());
            var outputPath = outputRoot / result.File;
            outputPaths.Add(outputPath);
            var dir = outputPath.Parent ?? throw new Exception("Should not happen");
            if (!Directory.Exists(dir.Normalized()))
            {
                Directory.CreateDirectory(dir.Normalized());
            }

            if (File.Exists(outputPath.Normalized()))
            {
                if (generator.SkipWriteIfFileExists())
                {
                    resultLogger.Debug("Skipped writing file because it already exists");
                    continue;
                }
            }

            var tempFile = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tmp");

            await using (var newFile = File.Create(tempFile))
            await using (var stream = await result.Content())
                await stream.CopyToAsync(newFile, ct);

            File.Move(tempFile, outputPath.Normalized(), overwrite: true);

            resultLogger.Debug("Written");

            sw.Stop();
            var relativeOutputPaths = outputPaths.Select(o => o.RelativeTo(outputRoot));
            logger.Info(
                $"Generated by {generator.GetType().Name} in {sw.ElapsedMilliseconds}ms ({relativeOutputPaths.Count()} file(s))");
        }
    }
}
