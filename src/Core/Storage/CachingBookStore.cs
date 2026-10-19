using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingBookStore(IBookStore inner) : IBookStore
{
    private readonly IBookStore _inner = inner
        ?? throw new ArgumentNullException(nameof(inner));

    private IReadOnlyList<BookCopy>? _cachedList;
    private readonly Dictionary<string, BookCopy?> _cachedById = new(StringComparer.OrdinalIgnoreCase);

    public int ListHits { get; private set; }
    public int ListMisses { get; private set; }
    public int GetByIdHits { get; private set; }
    public int GetByIdMisses { get; private set; }

    public IReadOnlyList<BookCopy> List()
    {
        if (_cachedList is null)
        {
            ListMisses++;
            _cachedList = _inner.List();
        }
        else
        {
            ListHits++;
        }

        return _cachedList;
    }

    public BookCopy? GetById(string id)
    {
        if (_cachedById.TryGetValue(id, out var cached))
        {
            GetByIdHits++;
            return cached;
        }

        GetByIdMisses++;
        var result = _inner.GetById(id);
        _cachedById[id] = result;
        return result;
    }

    public void Add(BookCopy item)
    {
        _inner.Add(item);
        InvalidateAll();
    }

    public void Update(BookCopy item)
    {
        _inner.Update(item);
        InvalidateAll();
    }

    public bool Remove(string id)
    {
        var result = _inner.Remove(id);
        if (result) InvalidateAll();
        return result;
    }

    private void InvalidateAll()
    {
        _cachedList = null;
        _cachedById.Clear();
    }
        public IReadOnlyList<BookCopy> Find(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _inner.Find(predicate);
    }
}