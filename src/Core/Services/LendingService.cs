using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class LendingService(IBookStore store)
{
    private readonly IBookStore _store = store
        ?? throw new ArgumentNullException(nameof(store));

    // Додати новий примірник
    public BookCopy AddBook(string isbn, string title, int year)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var copy = BookCopy.Create(id, isbn, title, year);
        _store.Add(copy);
        return copy;
    }

    // Видати примірник
    public void IssueCopy(string id)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        copy.Issue();
        _store.Update(copy);
    }

    // Повернути примірник
    public void ReturnCopy(string id)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        copy.Return();
        _store.Update(copy);
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string id) => _store.GetById(id);
    
    public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate) => _store.Find(predicate);
}