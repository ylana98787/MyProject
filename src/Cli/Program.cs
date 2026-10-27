using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Core.Notifications;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

// === COMPOSITION ROOT ===
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Фабрика створює базове сховище. Якщо є --cache, обгортає його в декоратор.
IBookStore store = StoreFactory.Create(args, dataPath);
var cachedStore = store as CachingBookStore;

// Виводимо інформацію про режими
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("MyProject - сервісний шар");
Console.WriteLine($"Базовий режим: {(args.Contains("--file") ? "FileBookStore (файл)" : "InMemoryBookStore (пам'ять)")}");
Console.WriteLine($"Кешування:     {(cachedStore != null ? "УВІМКНЕНО (CachingBookStore)" : "ВИМКНЕНО")}");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();

// === COMPOSITION ROOT: створюємо sink для сповіщень ===
var fileSink = new FileSink(Path.Combine(AppContext.BaseDirectory, "logs", "app.log"));
var consoleSink = new ConsoleSink();
var compositeSink = new CompositeSink(consoleSink, fileSink);

var service = new LendingService(store, compositeSink);
// === ПІДПИСКА НА ПОДІЮ ===
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// Перший підписник — локальний метод (щоб потім відписатися)
void OnLibraryChanged(object? sender, LibraryChangedEventArgs e)
{
    string details = e.Kind switch
    {
        LibraryChangeKind.BookAdded => "Додано новий примірник",
        LibraryChangeKind.CopyIssued => $"Видано читачу {e.ReaderId} до {e.DueDate:yyyy-MM-dd}",
        LibraryChangeKind.CopyReturned => "Повернено до бібліотеки",
        _ => "?"
    };
    var line = $"{e.At:HH:mm:ss} {e.Kind,-14} {e.CopyId} — {details}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}
service.Changed += OnLibraryChanged;

// Другий підписник — лямбда із замиканням
int eventCount = 0;
service.Changed += (_, _) => eventCount++;

// 1. Додати нову книгу
Console.WriteLine("--- Додати нову книгу ---");
var created = service.AddBook("978-966-04-0001-1", "Нова книга", 2024);
Console.WriteLine($"  Створено: {created}");
Console.WriteLine();

// 2. Видати
Console.WriteLine("--- Видати примірник ---");
service.IssueCopy(created.Id, "R-001", new DateTime(2026, 11, 1));
var issued = service.Find(created.Id);
Console.WriteLine($"  Після видачі: {issued}");
Console.WriteLine();

// 3. Повернути
Console.WriteLine("--- Повернути примірник ---");
service.ReturnCopy(created.Id);
var returned = service.Find(created.Id);
Console.WriteLine($"  Після повернення: {returned}");
Console.WriteLine();

// 4. Показати всі
Console.WriteLine("--- Список усіх примірників ---");
foreach (var b in service.All())
{
    Console.WriteLine($"  {b.Id,-10} {b.Isbn,-22} {b.Title,-30} {b.Year,5}  {b.Status}");
}
Console.WriteLine();

// 5. Пошук за id
Console.WriteLine("--- Пошук за id ---");
var found = service.Find(created.Id);
Console.WriteLine($"  Знайдено: {found}");
Console.WriteLine();

// === ПОШУК З FUNC ===
Console.WriteLine("=== Пошук з Func<BookCopy, bool> ===");

var available = service.Search(b => b.Status == CopyStatus.Available);
Console.WriteLine($"Доступні ({available.Count}):");
foreach (var b in available.Take(3))
    Console.WriteLine($"  {b.Id} — {b.Title}");

var century19 = service.Search(b => b.Year >= 1800 && b.Year < 1900);
Console.WriteLine();
Console.WriteLine($"19 століття ({century19.Count}):");
foreach (var b in century19)
    Console.WriteLine($"  {b.Year} — {b.Title}");

var kобзар = service.Search(b => b.Title.Contains("Кобзар", StringComparison.OrdinalIgnoreCase));
Console.WriteLine();
Console.WriteLine($"З 'Кобзар' у назві ({kобзар.Count}):");
foreach (var b in kобзар)
    Console.WriteLine($"  {b.Id} — {b.Title}");

Console.WriteLine();

// === СЦЕНАРІЙ ВІДМОВИ ===
Console.WriteLine("=== Сценарій відмови ===");

TryDo("дубль Id", () => store.Add(created));
TryDo("видати неіснуючий", () => service.IssueCopy("NO-SUCH-ID", "R-X", DateTime.Now));
TryDo("видати вже виданий", () =>
{
  service.IssueCopy(created.Id, "R-001", new DateTime(2026, 11, 1));
  service.IssueCopy(created.Id, "R-002", new DateTime(2026, 12, 1));
});

Console.WriteLine();

// === ДЕМОНСТРАЦІЯ КЕШУ ===
if (cachedStore != null)
{
    Console.WriteLine("=== Демонстрація кешу ===");
    Console.WriteLine("Викликаємо List() 3 рази поспіль:");

    for (int i = 1; i <= 3; i++)
    {
        _ = cachedStore.List();
        Console.WriteLine($"  #  {i}: hits={cachedStore.ListHits}, misses={cachedStore.ListMisses}");
    }

    Console.WriteLine("Робимо Add() — кеш має скинутися:");
    cachedStore.Add(BookCopy.Create(Guid.NewGuid().ToString("N")[..8], "000", "Тест кешу", 2000));
    _ = cachedStore.List();
    Console.WriteLine($"  hits={cachedStore.ListHits}, misses={cachedStore.ListMisses}");
}
else
{
    Console.WriteLine("=== Кешування вимкнено (немає --cache) ===");
}

Console.WriteLine();
Console.WriteLine("=".PadRight(70, '='));
// === СЦЕНАРІЙ ІЗ ВІДПИСКОЮ ===
Console.WriteLine();
Console.WriteLine("=== Відписка від події ===");
Console.WriteLine($"Подій отримано до відписки: {eventCount}");

service.Changed -= OnLibraryChanged;   // відписуємо локальний метод

Console.WriteLine("Відписалися від OnLibraryChanged. Наступна операція — тиха (без рядка в консолі та лог-файлі):");
service.ReturnCopy(created.Id);   // операція виконується, але повідомлення не пишеться (бо підписник відписався)
Console.WriteLine($"Подій після відписки (лямбда-лічильник): {eventCount}");

// === Func: пошук за предикатом ===
Console.WriteLine();
Console.WriteLine("=== FindAll з Func ===");
var few = service.FindAll(b => b.Year < 1900);   // книги до 1900 року
Console.WriteLine($"Книги до 1900 року ({few.Count}):");
foreach (var b in few.Take(5))
    Console.WriteLine($"  {b.Year} — {b.Title}");

    // === ДОДАТКОВЕ ЗАВДАННЯ 2: Action<string> для звіту про пропущені рядки ===
Console.WriteLine();
Console.WriteLine("=== Action<string> у імпорті CSV ===");

var csvPath = Path.Combine(AppContext.BaseDirectory, "data", "sample.csv");
// Якщо файлу немає у bin — беремо з кореня репозиторію
if (!File.Exists(csvPath))
    csvPath = Path.Combine("data", "sample.csv");

if (File.Exists(csvPath))
{
    Console.WriteLine($"Імпортуємо {csvPath}");
    
    // Передаємо лямбду — вона викликається для кожного пошкодженого рядка
    var csvResult = BookCsvImporter.Load(csvPath, error => 
        Console.WriteLine($"  [onError] {error}"));
    
    Console.WriteLine($"Завантажено: {csvResult.Items.Count}, помилок: {csvResult.Errors.Count}");
}
else
{
    Console.WriteLine("Файл sample.csv не знайдено (це нормально, якщо ви не копіювали його в bin)");
}

// === ДОПОМІЖНА ФУНКЦІЯ ===
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}