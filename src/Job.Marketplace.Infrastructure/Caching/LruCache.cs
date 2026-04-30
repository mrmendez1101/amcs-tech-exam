namespace Job.Marketplace.Infrastructure.Caching;

public sealed class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<Entry>> _map;
    private readonly LinkedList<Entry> _list = new();
    private readonly object _gate = new();

    public LruCache(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<Entry>>(capacity);
    }

    public int Count
    {
        get { lock (_gate) return _map.Count; }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        lock (_gate)
        {
            if (_map.TryGetValue(key, out var node))
            {
                _list.Remove(node);
                _list.AddFirst(node);
                value = node.Value.Value;
                return true;
            }
            value = default!;
            return false;
        }
    }

    public void Set(TKey key, TValue value)
    {
        lock (_gate)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                _list.Remove(existing);
                _map.Remove(key);
            }
            else if (_map.Count >= _capacity)
            {
                var lru = _list.Last!;
                _list.RemoveLast();
                _map.Remove(lru.Value.Key);
            }

            var node = new LinkedListNode<Entry>(new Entry(key, value));
            _list.AddFirst(node);
            _map[key] = node;
        }
    }

    public bool Remove(TKey key)
    {
        lock (_gate)
        {
            if (!_map.TryGetValue(key, out var node)) return false;
            _list.Remove(node);
            _map.Remove(key);
            return true;
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _map.Clear();
            _list.Clear();
        }
    }

    private readonly record struct Entry(TKey Key, TValue Value);
}
