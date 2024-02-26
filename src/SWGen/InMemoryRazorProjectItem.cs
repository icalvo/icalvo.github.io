using System.Text;
using RazorLight.Razor;

namespace SWGen;

public class InMemoryRazorProjectItem  : RazorLightProjectItem
{
    private readonly string _content;

    public InMemoryRazorProjectItem(string key, string content)
    {
        Key = key;
        _content = content;
        Exists = true;
    }

    public override Stream Read()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(_content));
    }

    public override string Key { get; }
    public override bool Exists { get; }
}