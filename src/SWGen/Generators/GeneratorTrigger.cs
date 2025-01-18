using SWGen.FileSystems;

namespace SWGen.Generators;

public abstract record GeneratorTrigger
{
    public record Once : GeneratorTrigger;
    public record OnFilePredicate(Func<AbsolutePathEx, RelativePathEx, bool> Predicate) : GeneratorTrigger;
}