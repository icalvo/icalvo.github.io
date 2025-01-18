using System.Collections.Generic;

namespace SWGen.Razor.Atom;

public interface IIndexBuilder<TIndex, TPost> where TPost : class, ICreatable<TPost>
{
    static abstract TIndex CreateIndex(IEnumerable<Document<TPost>> posts);
}