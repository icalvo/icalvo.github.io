namespace SWGen;

public abstract record GeneratorTrigger
{
    public record Once : GeneratorTrigger;
    public record OnFilePredicate(Func<AbsolutePathEx, RelativePathEx, bool> Predicate) : GeneratorTrigger;
}