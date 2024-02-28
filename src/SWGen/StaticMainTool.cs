using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;

namespace SWGen;

public static class StaticMainTool
{
    public static async Task<int> Process(string[] args, Config config, ILoader[] loaders)
    {
        var cwd = AbsolutePathEx.Create(Directory.GetCurrentDirectory());
        switch (args)
        {
            case ["build"]:
            {
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                await GuardedGenerate(config, loaders, projectRoot, outputRoot, CancellationToken.None);

                break;
            }
            case ["watch"]:
            {
                var projectRoot = cwd / "input";
                var outputRoot = cwd / "_public";
                await Watch(config, loaders, projectRoot, new ClientWebSocket(), outputRoot, CancellationToken.None);
                break;
            }
            default:
                throw new Exception("Unknown command");
        }

        return 0;
    }

    private static async Task GuardedGenerate(Config config, ILoader[] loaders, AbsolutePathEx projectRoot, AbsolutePathEx outputRoot, CancellationToken ct)
    {
        Logger.Info($"Input: {projectRoot}");
        Logger.Info($"Output: {outputRoot}");
        try
        {
            await GenerateFolder(projectRoot, isWatch: false, config, loaders, outputRoot, ct);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToStringDemystified());
        }
    }

    public static async Task Watch(Config cfg, ILoader[] loaders, AbsolutePathEx projectRoot, WebSocket webSocket, AbsolutePathEx outputRoot, CancellationToken ct)
    {
        var lastAccessed = new Dictionary<string, DateTime>();
        await GuardedGenerate(cfg, loaders, Directory.GetCurrentDirectory(), outputRoot, ct);
        
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
        
        SimpleWebServer(outputRoot);
        
        return;

        void Handler(object sender, FileSystemEventArgs args)
        {
            var pathDirectories = AbsolutePathEx.Create(args.FullPath).RelativeTo(projectRoot)
                ?? throw new Exception("Should not happen");
            var shouldHandle =
                !pathDirectories.Parts.Any(fragment => fragment is "_public" or ".git" or "bin" or "obj" or ".sass-cache" or ".ionide");
            if (shouldHandle)
            {
                var lastTimeWrite = File.GetLastWriteTime(args.FullPath);
                if (!lastAccessed.TryGetValue(args.FullPath, out var lastTimeAccessed) ||
                    Math.Abs((lastTimeAccessed - lastTimeWrite).Seconds) >= 1)
                {
                    Logger.Info($"Changes detected in {args.FullPath}, regenerating...");
                    lastAccessed[args.FullPath] = lastTimeWrite;
                    GuardedGenerate(cfg, loaders, Directory.GetCurrentDirectory(), outputRoot, ct).Wait(ct);
                }
            }
        }

        void ContentChangedHandler(object sender, FileSystemEventArgs args)
        {
            webSocket.SendAsync(ArraySegment<byte>.Empty, WebSocketMessageType.Close, endOfMessage:true, CancellationToken.None).Wait(ct);
        }
        
    }

    public static void SimpleWebServer(AbsolutePathEx localPath, params string[] prefixes)
    {
        if (!HttpListener.IsSupported)
        {
            Logger.Error("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
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
        Logger.Info($"Listening to {string.Join(", ",prefixes)}...");
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
    
    public static async Task GenerateFolder(AbsolutePathEx projectRoot, bool isWatch, Config config, ILoader[] loaders,
        AbsolutePathEx outputRoot, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var sc = new SiteContents();

        using (Logger.BeginScope("Load"))
        {
            foreach (var loader in loaders)
            {
                sc = await loader.Load(sc, projectRoot);
            }

            foreach (var error in sc.Errors())
            {
                Logger.Error($"{error.Path}: {error.Message}");
            }
        }

        using (Logger.BeginScope("Generation"))
        {
            using (Logger.BeginScope("Once"))
            {
                await RunOnceGenerators(config, sc, projectRoot, outputRoot, ct);
            }

            using (Logger.BeginScope("File based"))
            {
                foreach (var filePath in Directory.GetFiles(projectRoot.Normalized(), "*", new EnumerationOptions { RecurseSubdirectories = true })
                             .Select(AbsolutePathEx.Create))
                {
                    var relative = filePath.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
                    if (relative.Parts[0] is "bin" or "obj") continue;
                    if (sc.TryGetError(relative.Normalized()) != null) continue;

                    await Generate(config, sc, projectRoot, relative.Normalized(), outputRoot, ct);
                }
            }
        }

        Logger.Info($"Overall time: {sw.Elapsed}");
    }

    private static async Task RunOnceGenerators(Config cfg, SiteContents siteContent, AbsolutePathEx projectRoot,
        AbsolutePathEx outputRoot, CancellationToken ct)
    {
        foreach (var n in cfg.Generators.Where(n => n.Trigger is GeneratorTrigger.Once).Select(n => n.Generator))
        {
            await GenerateAux(n, siteContent, projectRoot, outputRoot, ct);
        }
    }

    private static async Task Generate(Config cfg, SiteContents siteContents, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, AbsolutePathEx outputRoot, CancellationToken ct)
    {
        siteContents.ContentAvailable = true;
        GeneratorConfig? pick = cfg.Generators.FirstOrDefault(n => n.MatchesFile(projectRoot, inputFile));
        if (pick == null)
        {
            Logger.Debug($"{inputFile} Ignored");
            return;
        }

        Logger.Debug($"Picked [{pick}] for {inputFile}");

        await GenerateAux(pick.Generator, siteContents, projectRoot, outputRoot, ct, inputFile);
    }

    private static async Task GenerateAux(Generator generator, SiteContents siteContents, AbsolutePathEx projectRoot,
        AbsolutePathEx outputRoot, CancellationToken ct, RelativePathEx? inputFile = null)
    {
        inputFile ??= RelativePathEx.Self();
        var sw = Stopwatch.StartNew();
        var results = await generator.Generate(siteContents, projectRoot, inputFile, ct).ToArrayAsync(ct);
        List<AbsolutePathEx> outputPaths = new();
        foreach (var result in results)
        {
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
                    continue;
                }
            }

            await using var newFile = File.Create(outputPath.Normalized());
            await result.Content.CopyToAsync(newFile, ct);
        }

        sw.Stop();
        var relativeOutputPaths = outputPaths.Select(o => o.RelativeTo(outputRoot));
        Logger.Info($"{inputFile} generated by {generator.GetType().Name} in {sw.ElapsedMilliseconds}ms -> [{relativeOutputPaths.StringJoin(",")}]");
    }
    
}

public static class StreamExtensions
{
    public static async Task<byte[]> ToByteArray(Stream input, CancellationToken ct = default)
    {
        if (input is MemoryStream ms) return ms.ToArray();
        ms = new MemoryStream();
        await input.CopyToAsync(ms, ct);
        return ms.ToArray();
    }
}
