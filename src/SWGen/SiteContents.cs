using System.Collections;
using System.ComponentModel.Design;

namespace SWGen;

public record SiteContents
{
    private readonly ServiceContainer _container = new();
    public bool ContentAvailable { get; set; }

    private readonly object _lock = new();
    public void Replace<T>(T value)
    {
        lock (_lock)
        {
            var key = typeof(List<T>);
            if (_container.GetService(key) != null)
            {
                _container.RemoveService(key);
            }

            _container.AddService(key, new List<T> { value });
        }
    }

    public void Add<T>(T value)
    {
        lock (_lock)
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
    }

    public IEnumerable<T> TryGetValues<T>()
    {
        lock (_lock)
        {
            var key = typeof(List<T>);
            if (_container.GetService(key) == null)
            {
                return Enumerable.Empty<T>();
            }

            return GetValues<T>();
        }
    }

    public T? TryGetValue<T>()
    {
        return GetValues<T>().SingleOrDefault();
    }

    private IEnumerable<T> GetValues<T>()
    {
        lock (_lock)
        {
            var key = typeof(List<T>);
            var service = _container.GetService(key);
            if (service == null)
            {
                return Enumerable.Empty<T>();
            }

            return ((IEnumerable<T>)service).ToArray();
        }
    }
}