using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryBookStore(IEnumerable<BookCopy>? seed = null) : IBookStore
{
    private readonly Dictionary<string, BookCopy> _items =
        (seed ?? []).ToDictionary(b => b.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<BookCopy> List() => _items.Values.ToList();

    public BookCopy? GetById(string id) => _items.GetValueOrDefault(id);

    public void Add(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
        _items.Add(item.Id, item);
    }

    public void Update(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.Id] = item;
    }
        public IReadOnlyList<BookCopy> Find(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }

    public bool Remove(string id) => _items.Remove(id);
}