using System.IO;
using SWGen.FileSystems;

namespace SWGen;

public record GeneratorItem(RelativePathEx File, Func<Task<Stream>> Content);