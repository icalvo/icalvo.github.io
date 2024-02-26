using Microsoft.AspNetCore.Html;
using RazorLight;
using RazorLight.Internal;

namespace CommandLine;

public interface ILayoutToggle
{
    public bool LayoutEnabled { get; set; }
}

public abstract class MyTemplatePage<TModel> : TemplatePage<TModel>, ILayoutToggle
{
    public bool LayoutEnabled { get; set; } = true;
    
    public new string Layout
    {
        get => base.Layout;
        set => base.Layout = LayoutEnabled ? value : null;
    }
}