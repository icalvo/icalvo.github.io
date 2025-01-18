using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using SWGen.FileSystems;
using SWGen.Generators;

namespace SWGen;

public class ApplicationService
{
    private readonly IFileSystem _fs;

    public ApplicationService(IFileSystem fs)
    {
        _fs = fs;
    }

    public Task Build(AbsolutePathEx projectRoot, AbsolutePathEx outputRoot, GeneratorConfig[] config, ILoader[] loaders, ISwgLogger logger)
        => GuardedGenerate(config, loaders, projectRoot, outputRoot, logger, CancellationToken.None);

    public Task Watch(AbsolutePathEx projectRoot, AbsolutePathEx outputRoot, GeneratorConfig[] config, ILoader[] loaders, ISwgLogger logger) =>
        Watch(
            config,
            loaders,
            projectRoot,
            new ClientWebSocket(),
            outputRoot,
            logger,
            CancellationToken.None);

    private async Task GuardedGenerate(GeneratorConfig[] config, ILoader[] loaders, AbsolutePathEx projectRoot,
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

    public async Task Watch(GeneratorConfig[] cfg, ILoader[] loaders, AbsolutePathEx projectRoot, WebSocket webSocket,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        // var lastAccessed = new Dictionary<string, DateTime>();
        // await GuardedGenerate(cfg, loaders, _fs.Directory.GetCurrentDirectory(), outputRoot, logger, ct);
        //
        // using var watcher = new FileSystemWatcher();
        // watcher.Path = projectRoot.Normalized();
        // watcher.EnableRaisingEvents = true;
        // watcher.IncludeSubdirectories = true;
        // watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.FileName;
        //
        // watcher.Created += Handler;
        // watcher.Changed += Handler;
        // watcher.Deleted += Handler;
        //
        //
        // watcher.Created += ContentChangedHandler;
        // watcher.Changed += ContentChangedHandler;
        // watcher.Deleted += ContentChangedHandler;
        //
        // SimpleWebServer(outputRoot, logger);

        return;

        // void Handler(object sender, FileSystemEventArgs args)
        // {
        //     var pathDirectories = AbsolutePathEx.Create(args.FullPath).RelativeTo(projectRoot) ??
        //                           throw new Exception("Should not happen");
        //     var shouldHandle = !pathDirectories.Parts.Any(
        //         fragment => fragment is "_public" or ".git" or "bin" or "obj" or ".sass-cache" or ".ionide");
        //     if (shouldHandle)
        //     {
        //         var lastTimeWrite = File.GetLastWriteTime(args.FullPath);
        //         if (!lastAccessed.TryGetValue(args.FullPath, out var lastTimeAccessed) ||
        //             Math.Abs((lastTimeAccessed - lastTimeWrite).Seconds) >= 1)
        //         {
        //             logger.Info($"Changes detected in {args.FullPath}, regenerating...");
        //             lastAccessed[args.FullPath] = lastTimeWrite;
        //             GuardedGenerate(cfg, loaders, Directory.GetCurrentDirectory(), outputRoot, logger, ct).Wait(ct);
        //         }
        //     }
        // }
        //
        // void ContentChangedHandler(object sender, FileSystemEventArgs args)
        // {
        //     webSocket.SendAsync(
        //         ArraySegment<byte>.Empty,
        //         WebSocketMessageType.Close,
        //         endOfMessage: true,
        //         CancellationToken.None).Wait(ct);
        // }

    }

    public async Task SimpleWebServer(AbsolutePathEx localPath, ISwgLogger logger, params string[] prefixes)
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
            if (await _fs.File.ExistsAsync(file))
            {
                await using var input = await _fs.File.OpenReadAsync(file);
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

    public async Task GenerateFolder(AbsolutePathEx projectRoot, bool isWatch, GeneratorConfig[] config, ILoader[] loaders,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var sc = new SiteContents();

        var pass = 0;
        const int maxPasses = 3;

        ILoader[] pendingLoaders = [];
        do
        {
            pass++;
            var loadLogger = logger.BeginScope($"Load (pass {pass})");
            (sc, pendingLoaders) = await loaders.ToAsyncEnumerable()
                .AggregateAwaitAsync(
                    (sc, PendingLoaders: Array.Empty<ILoader>()),
                    async (x, loader) =>
                    {
                        var loaderLogger = logger.BeginScope(loader.ToString() ?? "");
                        var (newsc, done) = await loader.Load(sc, projectRoot, loaderLogger, ct);

                        if (!done)
                        {
                            loaderLogger.Info("PENDING WORK");
                            return (newsc, [..x.PendingLoaders, loader]);
                        }

                        return (newsc, x.PendingLoaders);
                    }, ct);
            if (pass == maxPasses) break;
        } while (pendingLoaders.Any());

        if (pendingLoaders.Any())
        {
            var pendingLoadersList = pendingLoaders.StringJoin(", ");
            throw new Exception($"After {maxPasses} passes there are still pending loaders: {pendingLoadersList}");
        }

        var generateLogger = logger.BeginScope("Generation");
        await Parallel.ForEachAsync(
            _fs.Directory.GetFiles(projectRoot, "*", new System.IO.EnumerationOptions { RecurseSubdirectories = true }),
            ct,
            async (filePath, token) =>
            {
                var relative = filePath.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
                var fileGeneratorLogger = generateLogger.BeginScope(relative.Normalized(_fs));
                if (relative.Parts[0] is "bin" or "obj") return;

                await Generate(config, sc, projectRoot, relative.Normalized(_fs), outputRoot, fileGeneratorLogger, token);
            });

        logger.Info($"Overall time: {sw.Elapsed}");
    }

    private async Task Generate(GeneratorConfig[] cfg, SiteContents siteContents, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct)
    {
        siteContents.ContentAvailable = true;
        GeneratorConfig? pick = cfg.FirstOrDefault(n => n.MatchesFile(projectRoot, inputFile));
        if (pick == null)
        {
            logger.Debug("Ignored");
            return;
        }

        logger.Debug($"Picked {pick}");

        await GenerateAux(pick.Generator, siteContents, projectRoot, outputRoot, logger, ct, inputFile);
    }

    private async Task GenerateAux(Generator generator, SiteContents siteContents, AbsolutePathEx projectRoot,
        AbsolutePathEx outputRoot, ISwgLogger logger, CancellationToken ct, RelativePathEx? inputFile = null)
    {
        var tempDir = await _fs.Path.GetTempPath();
        inputFile ??= RelativePathEx.Self();
        var sw = Stopwatch.StartNew();
        var results = generator.Generate(siteContents, projectRoot, inputFile, logger, ct);
        List<AbsolutePathEx> outputPaths = [];
        foreach (var result in results)
        {
            var resultLogger = logger.BeginScope(result.File.Normalized(_fs));
            var outputPath = outputRoot / result.File;
            outputPaths.Add(outputPath);
            var dir = outputPath.Parent ?? throw new Exception("Should not happen");
            await _fs.Directory.CreateIfNotExistAsync(dir);

            if (await _fs.File.ExistsAsync(outputPath))
            {
                if (generator.SkipWriteIfFileExists())
                {
                    resultLogger.Debug("Skipped writing file because it already exists");
                    continue;
                }
            }

            var tempFile = tempDir / Guid.NewGuid().ToString("N") + ".tmp";

            await using (var newFile = await _fs.File.CreateAsync(tempFile))
            await using (var stream = await result.Content())
                await stream.CopyToAsync(newFile, ct);

            await _fs.File.MoveAsync(tempFile, outputPath, overwrite: true);

            resultLogger.Debug("Written");

            sw.Stop();
            if (outputPaths.Count > 1)
            {
                var relativeOutputPaths = outputPaths.Select(o => o.RelativeTo(outputRoot));
                logger.Info(
                    $"Generated by {generator.GetType().Name} in {sw.ElapsedMilliseconds}ms ({relativeOutputPaths.Count()} file(s))");
            }
            else
            {
                logger.Info(
                    $"Generated by {generator.GetType().Name} in {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}
