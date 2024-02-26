using SWGen;

namespace CommandLine;

public record IndexPage(Document<Post>[] PostPage, PageInfo? NextPage, PageInfo? PrevPage, PageInfo CurrentPage) : ICreatable<IndexPage>
{
    public static IndexPage Create()
    {
        return new IndexPage([], null, null, new PageInfo(1, "index.html"));
    }
}