using System.IO;
using SWGen.FileSystems;

namespace SWGen.Generators;

public record GeneratorItem(RelativePathEx File, Func<Task<Stream>> Content);