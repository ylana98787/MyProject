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
        //Якщо copy.Issue() кидає виняток, OnChanged не виконується. 
        // Тому подія не виникає. Саме це і демонструє сценарій відмови

        OnChanged(copy.Id, LibraryChangeKind.CopyIssued, readerId, dueDate);//// ← викликається лише якщо все успішно
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
    //А ось як цей метод оголошений. Він приймає параметр Func<BookCopy, bool> predicate.
//Func — це делегат, який повертає результат (у нашому випадку true або false).
//Всередині я беру весь список зі сховища (_store.List()) і за допомогою Where(predicate) залишаю тільки ті книги, для яких моя лямбда повернула true
    public IReadOnlyList<BookCopy> FindAll(Func<BookCopy, bool> predicate) => _store.List().Where(predicate).ToList();
    /// <summary>
/// Асинхронний імпорт книг із файлу через імпортер.
/// Імпортер передається як параметр (ручний DI).
/// </summary>
public async Task<int> ImportAsync(IBookImporter importer, string path, CancellationToken ct = default)
{
    // Асинхронно завантажуємо книги (всередині — FileStream з useAsync: true)
    var items = await importer.LoadAsync(path, ct);

    var added = 0;

    foreach (var item in items)
    {
        // Перевіряємо скасування на кожній ітерації
        ct.ThrowIfCancellationRequested();

        // Ідемпотентність: якщо запис з таким Id уже є — пропускаємо
        if (_store.GetById(item.Id) is not null)
            continue;

        // Додаємо в сховище
        _store.Add(item);

        // Піднімаємо подію (як на тижні 6) для кожного доданого запису
        OnChanged(item.Id, LibraryChangeKind.BookAdded);

        added++;
    }

    return added;
}
}