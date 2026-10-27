using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

// Реалізація сховища, яка зберігає дані у JSON-файлі.
// Для швидкості використовує кеш у пам'яті (щоб не читати файл щоразу).
public sealed class FileBookStore(string path) : IBookStore
{
    // Налаштування серіалізації JSON (спільне для всіх екземплярів).
    // WriteIndented = true — робить файл "гарним" (з відступами та переносами рядків).
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    // Кеш у пам'яті для швидкого доступу до даних.
    // StringComparer.OrdinalIgnoreCase — ігнорує регістр ключів.
    private readonly Dictionary<string, BookCopy> _cache = new(StringComparer.OrdinalIgnoreCase);
    
    // Абсолютний шлях до файлу (GetFullPath перетворює відносний шлях на повний).
    private readonly string _path = Path.GetFullPath(path);
    
    // Прапорець: чи дані вже завантажені з файлу в кеш.
    private bool _loaded;

    // Метод для завантаження даних з файлу в кеш.
    // Викликається при першому зверненні (ліниве завантаження).
    private void EnsureLoaded()
    {
        if (_loaded) return; // Якщо вже завантажено — нічого не робимо.

        if (File.Exists(_path)) // Якщо файл існує...
        {
            var json = File.ReadAllText(_path);
            // Десеріалізуємо JSON у список DTO (а не сутностей!).
            // ?? [] — якщо десеріалізація повернула null, беремо порожній список.
            var dtos = JsonSerializer.Deserialize<List<BookDto>>(json) ?? [];

            foreach (var dto in dtos)
            {
                // Конвертуємо DTO назад у сутність (проходить перевірки інваріантів).
                var book = BookCopy.FromDto(dto);
                _cache[book.Id] = book;
            }
        }

        _loaded = true; // Запам'ятовуємо, що дані завантажені.
    }

    // Метод для збереження даних з кешу у файл.
    // Викликається ПІСЛЯ кожної зміни (Add, Update, Remove).
    private void Flush()
    {
        // Створюємо директорію, якщо її немає (наприклад, папки data/).
        // ! (null-forgiving) — кажемо компілятору, що GetDirectoryName не поверне null.
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        
        // Перетворюємо сутності назад у DTO перед серіалізацією.
        // Select — це LINQ-проекція (перетворює кожен елемент).
        var dtos = _cache.Values.Select(b => b.ToDto()).ToList();
        
        // Серіалізуємо DTO у JSON і записуємо у файл.
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));
    }

    // Повертає всі записи (спочатку перевіряє, чи завантажено).
    public IReadOnlyList<BookCopy> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    // Пошук за Id (спочатку перевіряє, чи завантажено).
    public BookCopy? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    // Додавання нового запису.
    public void Add(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item); // Перевірка на null.
        EnsureLoaded(); // Завантажуємо дані, якщо ще не завантажені.

        // Перевірка на дублікат Id.
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

        _cache.Add(item.Id, item);
        Flush(); // Зберігаємо зміни на диск.
    }

    // Оновлення існуючого запису.
    public void Update(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        _cache[item.Id] = item; // Оновлюємо в кеші.
        Flush(); // Зберігаємо на диск.
    }

    // Видалення запису за Id.
    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false; // Якщо не знайдено — повертаємо false.
        Flush(); // Якщо видалено — зберігаємо зміни.
        return true;
    }

    // Пошук за предикатом.
    public IReadOnlyList<BookCopy> Find(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}