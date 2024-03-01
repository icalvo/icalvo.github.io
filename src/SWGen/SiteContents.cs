using System.Collections;

namespace SWGen;

public record SiteContents
{
    private readonly Dictionary<Type, IList> _container = new();
    public bool ContentAvailable { get; set; }

    private readonly object _lock = new();
    public void Replace<T>(T value)
    {
        lock (_lock)
        {
            var key = typeof(List<T>);
            _container[key] = new List<T> { value };
        }
    }

    public void Add<T>(T value)
    {
        lock (_lock)
        {
            var key = typeof(List<T>);
            if (_container.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                _container[key] = new List<T> { value };
            }
        }
    }

    public IEnumerable<T> TryGetValues<T>()
    {
        lock (_lock)
        {
            return
                _container.TryGetValue(typeof(List<T>), out var list)
                    ? ((IEnumerable<T>)list).ToArray()
                    : Enumerable.Empty<T>();
        }
    }

    public T? TryGetValue<T>()
    {
        return TryGetValues<T>().SingleOrDefault();
    }
}