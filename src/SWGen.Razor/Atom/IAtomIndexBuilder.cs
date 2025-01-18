using System.Collections.Generic;

namespace SWGen.Razor.Atom;

public interface IAtomIndexBuilder<TIndex, TPost> where TPost : class, IDated, ICreatable<TPost>
{
    static abstract TIndex CreateIndex(IEnumerable<Document<TPost>> posts);
}