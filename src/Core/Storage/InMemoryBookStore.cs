using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Реалізація сховища, яка зберігає дані в оперативній пам'яті (у словнику).
// Дані зникають після завершення роботи програми.
// "primary constructor" (параметри в дужках після імені класу) — це сучасний синтаксис C# 12.
// seed — початкові дані (необов'язкові). Якщо null, словник буде порожнім.
public sealed class InMemoryBookStore(IEnumerable<BookCopy>? seed = null) : IBookStore
{
    // Приватний словник (ключ — Id, значення — сутність).
    // StringComparer.OrdinalIgnoreCase — ігнорує регістр при порівнянні ключів (наприклад, "bc-001" і "BC-001" це один ключ).
    // seed ?? [] — якщо seed == null, беремо порожню колекцію.
    // ToDictionary — перетворює колекцію на словник.
    private readonly Dictionary<string, BookCopy> _items =
        (seed ?? []).ToDictionary(b => b.Id, StringComparer.OrdinalIgnoreCase);

    // Повертає копію всіх записів у вигляді IReadOnlyList (захист від зовнішніх змін).
    public IReadOnlyList<BookCopy> List() => _items.Values.ToList();

    // Шукає запис за Id. Повертає null, якщо не знайдено (тому BookCopy?).
    // GetValueOrDefault — безпечний метод для словника (не кидає виняток, а повертає null).
    public BookCopy? GetById(string id) => _items.GetValueOrDefault(id);

    // Додає новий запис.
    public void Add(BookCopy item)
    {
        // Якщо item == null — кидає ArgumentNullException.
        ArgumentNullException.ThrowIfNull(item);
        
        // Перевірка на дублікат Id (унікальність — це інваріант колекції).
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
            
        // Додаємо в словник (ключ = Id).
        _items.Add(item.Id, item);
    }

    // Оновлює існуючий запис (або додає, якщо такого Id ще немає).
    public void Update(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        // Індексатор [key] = value — перезаписує значення за ключем.
        _items[item.Id] = item;
    }

    // Пошук за предикатом (умовою). Наприклад: "знайти всі доступні книги".
    // LINQ .Where(predicate) — фільтрує елементи.
    public IReadOnlyList<BookCopy> Find(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }

    // Видаляє запис за Id. Повертає true, якщо видалено, false — якщо не знайдено.
    public bool Remove(string id) => _items.Remove(id);
}