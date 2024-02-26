using System.ComponentModel.Design;

namespace SWGen;

public record SiteContents
{
    private readonly ServiceContainer _container = new();
    private readonly Dictionary<string, SiteError> _errors = new();
    public bool ContentAvailable { get; set; }

    public void AddError(SiteError error)
    {
        _errors.Add(error.Path, error);
    }
    
    public void Add<T>(T value)
    {
        var key = typeof(List<T>);
        switch (_container.GetService(key))
        {
            case List<T> list: list.Add(value);
                break;
            default: _container.AddService(key, new List<T> { value });
                break;
        }
    }

    public SiteError? TryGetError(string page) => _errors.GetValueOrDefault(page);

    public IEnumerable<T> TryGetValues<T>()
    {
        var key = typeof(List<T>);
        if (_container.GetService(key) == null)
        {
            return Enumerable.Empty<T>();
        }

        return GetValues<T>();
    }
    
    public IEnumerable<T> GetValues<T>()
    {
        var key = typeof(List<T>);
        var service = _container.GetService(key);
        if (service == null)
        {
            return Enumerable.Empty<T>();
        }

        return (IEnumerable<T>)service;
    }

    public T? TryGetValue<T>()
    {
        return GetValues<T>().SingleOrDefault();
    }

    public IEnumerable<SiteError> Errors()
    {
        return _errors.Values;
    }
}