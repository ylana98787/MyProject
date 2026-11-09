using System.Text;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Core.Notifications;
using Core.Import;
using Core.Reports;

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
// === ІМПОРТ ЧЕРЕЗ ASYNC ===
if (args.Contains("import"))
{
    // Шлях до файлу: наступний аргумент після "import", або data/sample.json
    var importPath = args.SkipWhile(a => a != "import").Skip(1).FirstOrDefault()
                     ?? Path.Combine("data", "sample.json");

    Console.WriteLine($"=== Асинхронний імпорт: {importPath} ===");

    // CancellationTokenSource для Ctrl+C
    using var cts = new CancellationTokenSource();
    
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;   // Забороняємо стандартне завершення
        cts.Cancel();       // Сигналізуємо скасування
        Console.WriteLine("\n[Скасування...]");
    };

    // Для демонстрації таймауту — розкоментуйте наступний рядок:
    cts.CancelAfter(TimeSpan.FromMilliseconds(2));

    try
    {
        var added = await service.ImportAsync(
            new JsonBookImporter(),
            importPath,
            cts.Token);

        Console.WriteLine($"Імпортовано записів: {added}");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Імпорт скасовано.");
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"Дані відхилено: {ex.Message}");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Файл не знайдено: {importPath}");
    }

    return 0;   // ← виходимо після імпорту
}
// === ПІДКОМАНДИ ЗВІТІВ ===
var command = args.FirstOrDefault(a => !a.StartsWith("--"));

if (command is not null && command.StartsWith("report-"))
{
    var allItems = service.All();  // ← СЕРВІС МАЄ БУТИ ВЖЕ СТВОРЕНИЙ!

    switch (command)
    {
        case "report-top":
            Console.WriteLine("=== Топ-5 найстаріших книг ===");
            PrintLines(ReportService.TopOldest(allItems, 5));
            break;

        case "report-low":
            Console.WriteLine("=== Книги, видані до 1900 року ===");
            PrintLines(ReportService.OldBooks(allItems, 1900));
            break;

        case "report-groups":
            Console.WriteLine("=== Групування за статусом ===");
            foreach (var g in ReportService.ByStatus(allItems))
                Console.WriteLine($"  {g.Key,-10} книг: {g.Count,3}");
            break;

        case "report-sum":
            var t = ReportService.Totals(allItems);
            Console.WriteLine("=== Підсумки ===");
            Console.WriteLine($"  Всього книг: {t.Items}");
            Console.WriteLine($"  Середній рік: {t.AverageYear:F0}");
            Console.WriteLine($"  Найстаріша: {t.OldestYear}");
            Console.WriteLine($"  Найновіша: {t.NewestYear}");
            break;

            case "report-deferred":
            Console.WriteLine("=== Відкладене виконання LINQ ===");
            //1. Беремо знімок джерела (список книг)
            var source = allItems.ToList();
            Console.WriteLine($"Початкова кількість: {source.Count}");
            //2. Будуємо запит (ще НЕ виконується)
            var query = source.Where(b => b.Year < 1900);
            // 3. Перший Count() — запит виконується, повертає 10
            Console.WriteLine($"Стародавніх до змін: {query.Count()}");
            // 4. Змінюємо джерело (додаємо книгу з роком 1800)
            source.Add(BookCopy.Create("TMP-001", "000-000", "Нова книга", 1800));
            Console.WriteLine($"У джерелі тепер: {source.Count}");
            // 5. Другий Count() — запит виконується ЗНОВУ, повертає 11
            Console.WriteLine($"Стародавніх після змін: {query.Count()}");
            // 6. Матеріалізуємо запит (фіксуємо результат)
            var snapshot = query.ToList();
            Console.WriteLine($"У знімку (snapshot): {snapshot.Count}");

            source.Clear();
            Console.WriteLine($"У знімку після Clear: {snapshot.Count}");
            break;

            case "report-join":
            Console.WriteLine("=== Зведений звіт видач (Join) ===");
    
    // Створимо демонстраційні видачі для прикладу
    var demoCopy1 = allItems.FirstOrDefault(b => b.Id == "BC-001");
    var demoCopy2 = allItems.FirstOrDefault(b => b.Id == "BC-002");
    
    if (demoCopy1 == null || demoCopy2 == null)
    {
        Console.WriteLine("Потрібні BC-001 і BC-002 — перевірте SampleData.");
        break;
    }
    
    var demoLoans = new List<Loan>
    {
        Loan.Open("L-001", demoCopy1, "R-001", new DateTime(2026, 10, 1)),
        Loan.Open("L-002", demoCopy2, "R-002", new DateTime(2026, 10, 5))
    };
    
    var joined = ReportService.JoinedLoans(allItems, demoLoans);
    foreach (var j in joined)
        Console.WriteLine($"  {j.CopyId,-10} {j.Title,-30} → {j.ReaderId} (до {j.IssuedOn:yyyy-MM-dd})");
    break;

    case "report-newest":
    Console.WriteLine("=== Топ-5 найновіших книг (TopBy) ===");
    PrintLines(ReportService.TopNewest(allItems, 5));
    break;

    case "report-longest":
    Console.WriteLine("=== Топ-5 найдовших назв (TopBy) ===");
    PrintLines(ReportService.TopByLongestTitle(allItems, 5));
    break;

       case "report-groups-qs":
        Console.WriteLine("=== Групування за статусом (query syntax) ===");
        foreach (var g in ReportService.ByStatusQuerySyntax(allItems))
            Console.WriteLine($"  {g.Key,-10} книг: {g.Count,3}");
        break;

        case "report-perf":
    Console.WriteLine("=== Вимірювання продуктивності (100 000 записів) ===");
    
    var random = new Random(42);
    var bigData = Enumerable.Range(1, 100_000)
        .Select(i => BookCopy.Create(
            $"BIG-{i:D5}", 
            $"978-966-{i:D5}-1", 
            $"Книга {i}", 
            random.Next(1450, 2026)))
        .ToList();
    
    Console.WriteLine($"Згенеровано: {bigData.Count} записів");
    
    // Варіант 1: БЕЗ ToList у середині
    var sw1 = System.Diagnostics.Stopwatch.StartNew();
    var result1 = bigData
        .Where(b => b.Year < 1900)
        .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
        .ToList();
    sw1.Stop();
    Console.WriteLine($"  Без ToList у середині:  {sw1.ElapsedMilliseconds} ms, результат: {result1.Count}");
    
    // Варіант 2: З ToList у середині
    var sw2 = System.Diagnostics.Stopwatch.StartNew();
    var result2 = bigData
        .Where(b => b.Year < 1900)
        .ToList()
        .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
        .ToList();
    sw2.Stop();
    Console.WriteLine($"  З ToList у середині:    {sw2.ElapsedMilliseconds} ms, результат: {result2.Count}");
    
    Console.WriteLine($"  Різниця: {sw2.ElapsedMilliseconds - sw1.ElapsedMilliseconds} ms");
    break;

    default:
        Console.WriteLine("Доступні команди: report-top | report-low | report-groups | report-sum | report-deferred | report-join | report-newest | report-longest | report-groups-qs");
        break;
            }


    return 0;   
}

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
service.Changed += OnLibraryChanged;//підписка локального методу. Він пише в консоль і в logs/app.log.
// Локальний метод обраний, бо його можна відписати через -= (на відміну від лямбди "в один рядок").

// Другий підписник — лямбда із замиканням
int eventCount = 0;
service.Changed += (_, _) => eventCount++;//другий підписник, лямбда. 
//Вона рахує події через замикання над змінною eventCount.

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
//Ось я викликаю метод FindAll і передаю йому лямбду b => b.Year < 1900.

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
static void PrintLines(IReadOnlyList<BookReportLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($"  {l.Id,-10} {l.Year,5}  {l.Title,-30}  [{l.Status}]");
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

return 0; 