using SWGen.Generators;

namespace SWGen;

public record SiteError(string Path, string Message, GenerationPhase Phase);