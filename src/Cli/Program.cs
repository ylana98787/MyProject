using System.Runtime.InteropServices;
using System.Text;

// Встановлюємо кодування для коректного відображення кирилиці
Console.OutputEncoding = Encoding.UTF8;

// Заголовок
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("MyProject - Наскрізний проєкт з крос-платформного програмування");
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine();

// 1. Інформація про операційну систему
Console.WriteLine($"{"ОС (OSDescription)",-30}: {RuntimeInformation.OSDescription}");
Console.WriteLine($"{"ОС (Environment)",-30}: {Environment.OSVersion}");
Console.WriteLine($"{"Архітектура процесу",-30}: {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"{"Архітектура ОС",-30}: {RuntimeInformation.OSArchitecture}");

// 2. Інформація про .NET
Console.WriteLine($"{"Версія .NET (CLR)",-30}: {Environment.Version}");
Console.WriteLine($"{"Runtime",-30}: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"{"RID",-30}: {RuntimeInformation.RuntimeIdentifier}");

// 3. Інформація про каталоги
Console.WriteLine($"{"Каталог застосунку",-30}: {AppContext.BaseDirectory}");
Console.WriteLine($"{"Поточний каталог",-30}: {Environment.CurrentDirectory}");

Console.WriteLine();
Console.WriteLine("=".PadRight(60, '='));

// 4. Предметна область — Бібліотека
Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
Console.WriteLine("Призначення: облік видач примірників книг читачам та їх повернень");
Console.WriteLine("=".PadRight(60, '='));

// 5. Додаткове завдання — підтримка --json
if (args.Length > 0 && args[0] == "--json")
{
    Console.WriteLine();
    Console.WriteLine("{\"status\": \"JSON mode active\"}");
}