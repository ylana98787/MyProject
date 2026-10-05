using System.Text;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

// Шлях до файлу: args[0] або data/sample.csv
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// ===== СПЕЦІАЛЬНИЙ ВИПАДОК: змішаний імпорт =====
if (path.EndsWith("_mixed.csv", StringComparison.OrdinalIgnoreCase))
{
    var mixedResult = MixedCsvImporter.Load(path);

    Console.WriteLine("=".PadRight(70, '='));
    Console.WriteLine("MyProject - змішаний імпорт");
    Console.WriteLine("Студент: Новосельська Уляна Василівна, група ФЕІ-35");
    Console.WriteLine("=".PadRight(70, '='));
    Console.WriteLine();
    Console.WriteLine($"Завантажено записів: {mixedResult.Items.Count}");
    Console.WriteLine();

    foreach (var item in mixedResult.Items.Take(10))
    {
        Console.WriteLine(item switch
        {
            BookDto b => $"  [BOOK]   {b.Id,-6} {b.Title,-30} {b.Year}",
            ReaderDto r => $"  [READER] {r.Id,-6} {r.FullName}",
            _ => $"  [UNKNOWN] {item}"
        });
    }

    Console.WriteLine();
    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine($"Пропущено рядків: {mixedResult.Errors.Count}");
        foreach (string e in mixedResult.Errors)
            Console.WriteLine($"  ! {e}");
    }
    else
    {
        Console.WriteLine("Помилок не знайдено.");
    }

    Console.WriteLine();
    Console.WriteLine("=".PadRight(70, '='));
    return 0;
}

// ===== ЗВИЧАЙНИЙ ІМПОРТ (CSV або JSON) =====
string ext = Path.GetExtension(path).ToLowerInvariant();

ImportResult<BookDto> result = ext switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    _ => new ImportResult<BookDto>([], [$"невідомий формат файлу: {ext}"])
};

Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("MyProject - імпорт книг");
Console.WriteLine("Студент: Новосельська Уляна Василівна, група ФЕІ-35");
Console.WriteLine($"Формат: {ext}");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine();

foreach (BookDto b in result.Items.Take(5))
{
    Console.WriteLine($"  {b.Id,-6} {b.Isbn,-22} {b.Title,-30} {b.Year,5}");
}

Console.WriteLine();
int total = result.Items.Count + result.Errors.Count;
int accepted = result.Items.Count;
int skipped = result.Errors.Count;
double errorPercent = total > 0 ? skipped * 100.0 / total : 0;
Console.WriteLine($"Статистика: всього {total} | прийнято {accepted} | пропущено {skipped} | помилок {errorPercent:F1}%");
Console.WriteLine();

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}
else
{
    Console.WriteLine("Помилок не знайдено.");
}

Console.WriteLine();
Console.WriteLine("=".PadRight(70, '='));

return 0;