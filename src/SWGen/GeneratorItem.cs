namespace SWGen;

public record GeneratorItem(RelativePathEx File, Func<Task<Stream>> Content);