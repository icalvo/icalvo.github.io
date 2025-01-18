using SWGen;
using SWGen.Razor;
using SWGen.Razor.Atom;

namespace CommandLine;

public record IndexPage(Document<Post>[] PostPage, PageInfo? NextPage, PageInfo? PrevPage, PageInfo CurrentPage)
    : ICreatable<IndexPage>, IAtomIndexBuilder<IndexPage, Post>
{
    public static IndexPage Create()
    {
        return new IndexPage([], null, null, new PageInfo(1, "index.html"));
    }

    public static IndexPage CreateIndex(IEnumerable<Document<Post>> posts)
    {
        return new IndexPage(posts.ToArray(), null, null, new PageInfo(0, "---"));
    }
}