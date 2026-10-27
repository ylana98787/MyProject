using Core.Abstractions;
using Core.Domain;
using Core.Notifications;

namespace Core.Services;

public sealed class LendingService(IBookStore store, INotificationSink? sink = null)
{
    private readonly IBookStore _store = store
        ?? throw new ArgumentNullException(nameof(store));

    // НОВЕ: залежність для сповіщень через DI (ручний)
    private readonly INotificationSink? _sink = sink;

    // Подія для сповіщення (Observer)
    public event EventHandler<LibraryChangedEventArgs>? Changed;

    // Приватний метод підняття події ТА відправки в sink
    private void OnChanged(string copyId, LibraryChangeKind kind, string? readerId = null, DateTime? dueDate = null)
    {
        var args = new LibraryChangedEventArgs(copyId, kind, readerId, dueDate);
        
        // 1. Викликаємо подію (якщо є підписники)
        Changed?.Invoke(this, args);
        
        // 2. Викликаємо sink (якщо він переданий)
        _sink?.Send(args);
    }

    public BookCopy AddBook(string isbn, string title, int year)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var copy = BookCopy.Create(id, isbn, title, year);
        _store.Add(copy);

        // Подія генерується ПІСЛЯ успішного додавання
        OnChanged(copy.Id, LibraryChangeKind.BookAdded);

        return copy;
    }

    public void IssueCopy(string id, string readerId, DateTime dueDate)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        // Якщо Issue() кине виняток — подія НЕ згенерується
        copy.Issue();
        _store.Update(copy);

        OnChanged(copy.Id, LibraryChangeKind.CopyIssued, readerId, dueDate);
    }

    public void ReturnCopy(string id)
    {
        var copy = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає примірника з id={id}.");

        copy.Return();
        _store.Update(copy);

        OnChanged(copy.Id, LibraryChangeKind.CopyReturned);
    }

    public IReadOnlyList<BookCopy> All() => _store.List();

    public BookCopy? Find(string id) => _store.GetById(id);

    public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate) => _store.Find(predicate);

    // Для демонстрації Func (додаткове завдання лабораторної 5)
    public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate) => _store.List().Where(predicate).ToList();
}