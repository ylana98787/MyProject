using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

// === COMPOSITION ROOT: єдине місце з конкретними класами ===
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.Books());

var service = new LendingService(store);

// === СЦЕНАРІЙ ===
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("MyProject - сервісний шар");
Console.WriteLine($"Сховище: {store.GetType().Name}");
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
TryDo("дубль Id", () => store.Add(created));

// Спроба видати неіснуючий
TryDo("видати неіснуючий", () => service.IssueCopy("NO-SUCH-ID"));

// Спроба видати вже виданий
TryDo("видати вже виданий", () =>
{
    service.IssueCopy(created.Id);   // перше — OK
    service.IssueCopy(created.Id);   // друге — помилка
});

Console.WriteLine();
Console.WriteLine("=".PadRight(70, '='));

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