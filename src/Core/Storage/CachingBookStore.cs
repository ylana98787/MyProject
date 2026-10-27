using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Декоратор для сховища, який додає кешування.
// Він приймає будь-яке інше сховище (inner) і зберігає результати його викликів у пам'яті.
public sealed class CachingBookStore(IBookStore inner) : IBookStore
{
    // Внутрішнє сховище, яке ми обгортаємо. Захист від null.
    private readonly IBookStore _inner = inner
        ?? throw new ArgumentNullException(nameof(inner));

    // Кеш для всього списку (List).
    // null означає, що кеш порожній і треба звертатися до inner.
    private IReadOnlyList<BookCopy>? _cachedList;

    // Кеш для окремих записів (GetById).
    // Зберігає навіть null (якщо inner повернув null, ми це запам'ятаємо).
    private readonly Dictionary<string, BookCopy?> _cachedById = new(StringComparer.OrdinalIgnoreCase);

    // Лічильники для демонстрації ефективності кешу (скільки разів влучили/не влучили).
    // private set означає, що змінювати їх можна лише всередині цього класу.
    public int ListHits { get; private set; }
    public int ListMisses { get; private set; }
    public int GetByIdHits { get; private set; }
    public int GetByIdMisses { get; private set; }

    // Отримати всі записи (з кешуванням).
    public IReadOnlyList<BookCopy> List()
    {
        // Якщо кеш порожній (перший виклик або після змін)
        if (_cachedList is null)
        {
            ListMisses++;
            _cachedList = _inner.List(); // Звертаємося до базового сховища
        }
        else
        {
            ListHits++; // Беремо з кешу (швидко)
        }

        return _cachedList;
    }

    // Знайти запис за Id (з кешуванням).
    public BookCopy? GetById(string id)
    {
        // TryGetValue перевіряє, чи є ключ у словнику.
        if (_cachedById.TryGetValue(id, out var cached))
        {
            GetByIdHits++;
            return cached; // Знайшли в кеші (навіть якщо там null)
        }

        GetByIdMisses++;
        var result = _inner.GetById(id); // Звертаємося до базового сховища
        _cachedById[id] = result;        // Запам'ятовуємо результат
        return result;
    }

    // Додавання запису.
    public void Add(BookCopy item)
    {
        _inner.Add(item); // Додаємо в базове сховище
        InvalidateAll();  // Очищаємо кеш, бо дані змінилися
    }

    // Оновлення запису.
    public void Update(BookCopy item)
    {
        _inner.Update(item);
        InvalidateAll(); // Очищаємо кеш
    }

    // Видалення запису.
    public bool Remove(string id)
    {
        var result = _inner.Remove(id);
        if (result) InvalidateAll(); // Очищаємо кеш, тільки якщо щось видалили
        return result;
    }

    // Скидання кешу (викликається при будь-яких змінах).
    private void InvalidateAll()
    {
        _cachedList = null;
        _cachedById.Clear();
    }

    // Пошук за предикатом.
    public IReadOnlyList<BookCopy> Find(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        // Кешувати Find складно (бо умови різні), тому просто делегуємо inner.
        return _inner.Find(predicate);
    }
}