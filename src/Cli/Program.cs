using System.Runtime.InteropServices;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length > 0 && args[0] == "--json")
{
    // JSON-режим
    var info = new
    {
        os = RuntimeInformation.OSDescription,
        osVersion = Environment.OSVersion.ToString(),
        architecture = RuntimeInformation.ProcessArchitecture.ToString(),
        osArchitecture = RuntimeInformation.OSArchitecture.ToString(),
        clrVersion = Environment.Version.ToString(),
        runtime = RuntimeInformation.FrameworkDescription,
        rid = RuntimeInformation.RuntimeIdentifier,
        appDirectory = AppContext.BaseDirectory,
        currentDirectory = Environment.CurrentDirectory,
        domain = "Бібліотека",
        entities = new[] { "Book", "BookCopy", "Reader", "Loan" },
        purpose = "облік видач примірників книг читачам та їх повернень"
    };

    var options = new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(info, options));
}
else
{
    // Табличний режим
    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine("MyProject - Наскрізний проєкт з крос-платформного програмування");
     Console.WriteLine("Студент: Новосельська Уляна Василівна, група ФЕІ-35");
    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine();

    Console.WriteLine($"{"ОС (OSDescription)",-30}: {RuntimeInformation.OSDescription}");
    Console.WriteLine($"{"ОС (Environment)",-30}: {Environment.OSVersion}");
    Console.WriteLine($"{"Архітектура процесу",-30}: {RuntimeInformation.ProcessArchitecture}");
    Console.WriteLine($"{"Архітектура ОС",-30}: {RuntimeInformation.OSArchitecture}");
    Console.WriteLine($"{"Версія .NET (CLR)",-30}: {Environment.Version}");
    Console.WriteLine($"{"Runtime",-30}: {RuntimeInformation.FrameworkDescription}");
    Console.WriteLine($"{"RID",-30}: {RuntimeInformation.RuntimeIdentifier}");
    Console.WriteLine($"{"Каталог застосунку",-30}: {AppContext.BaseDirectory}");
    Console.WriteLine($"{"Поточний каталог",-30}: {Environment.CurrentDirectory}");

    Console.WriteLine();
    Console.WriteLine("=".PadRight(60, '='));
    Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
    Console.WriteLine("Призначення: облік видач примірників книг читачам та їх повернень");
    Console.WriteLine("=".PadRight(60, '='));
}