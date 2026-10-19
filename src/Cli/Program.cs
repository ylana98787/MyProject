using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// === COMPOSITION ROOT ===
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

var cachedStore = (CachingBookStore)StoreFactory.Create(args, dataPath);
var baseStore = cachedStore;
var service = new LendingService(cachedStore);

// === СЦЕНАРІЙ ===
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("MyProject - сервісний шар");
Console.WriteLine($"Сховище: {cachedStore.GetType().Name}");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();

// 1. Додати нову книгу
Console.WriteLine("--- Додати нову книгу ---");
var created = service.AddBook("978-966-04-0001-1", "Нова книга", 2024);
Console.WriteLine($"  Створено: {created}");
Console.WriteLine();

// 2. Видати
Console.WriteLine("--- Видати примірник ---");
service.IssueCopy(created.Id);
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

// === СЦЕНАРІЙ ВІДМОВИ ===
Console.WriteLine("=== Сценарій відмови ===");

// Спроба створити книгу з дублем Id (через пряме додавання в store)
TryDo("дубль Id", () => cachedStore.Add(created));

// Спроба видати неіснуючий
TryDo("видати неіснуючий", () => service.IssueCopy("NO-SUCH-ID"));

// Спроба видати вже виданий
TryDo("видати вже виданий", () =>
{
    service.IssueCopy(created.Id);   // перше — OK
    service.IssueCopy(created.Id);   // друге — помилка
});

Console.WriteLine();
Console.WriteLine("=== Демонстрація кешу ===");
Console.WriteLine("Викликаємо All() 3 рази поспіль:");

for (int i = 1; i <= 3; i++)
{
    _ = cachedStore.List();
    Console.WriteLine($"  #  {i}: hits={cachedStore.ListHits}, misses={cachedStore.ListMisses}");
}

Console.WriteLine("Після Add() кеш скидається:");
_ = cachedStore.List();
Console.WriteLine($"  hits={cachedStore.ListHits}, misses={cachedStore.ListMisses}");
Console.WriteLine();
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();
Console.WriteLine("=== Пошук з Func<BookCopy, bool> ===");

// Пошук 1: всі доступні
var available = service.Search(b => b.Status == CopyStatus.Available);
Console.WriteLine($"Доступні ({available.Count}):");
foreach (var b in available.Take(3))
    Console.WriteLine($"  {b.Id} — {b.Title}");

// Пошук 2: книги 19 століття (1800-1899)
var century19 = service.Search(b => b.Year >= 1800 && b.Year < 1900);
Console.WriteLine();
Console.WriteLine($"19 століття ({century19.Count}):");
foreach (var b in century19)
    Console.WriteLine($"  {b.Year} — {b.Title}");

// Пошук 3: книги з "Кобзар" у назві
var kобзар = service.Search(b => b.Title.Contains("Кобзар", StringComparison.OrdinalIgnoreCase));
Console.WriteLine();
Console.WriteLine($"З 'Кобзар' у назві ({kобзар.Count}):");
foreach (var b in kобзар)
    Console.WriteLine($"  {b.Id} — {b.Title}");

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