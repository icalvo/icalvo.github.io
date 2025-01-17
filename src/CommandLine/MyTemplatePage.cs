using RazorLight;
using SWGen.Razor;

namespace CommandLine;

public abstract class MyTemplatePage<TModel> : TemplatePage<TModel>, ILayoutToggle
{
    public bool LayoutEnabled { get; set; } = true;
    
    public new string Layout
    {
        get => base.Layout;
        set => base.Layout = LayoutEnabled ? value : null;
    }
}