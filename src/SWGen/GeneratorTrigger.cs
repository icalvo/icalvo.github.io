namespace SWGen;

public abstract record GeneratorTrigger
{
    public record Once : GeneratorTrigger;
    public record OnFile(RelativePathEx FileName) : GeneratorTrigger;
    public record OnFileExt(string Extension) : GeneratorTrigger;
    public record OnFilePredicate(Func<AbsolutePathEx, RelativePathEx, bool> Predicate) : GeneratorTrigger;
}