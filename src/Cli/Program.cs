using System.Text;
using Core;

Console.OutputEncoding = Encoding.UTF8;

// Збираємо інформацію про середовище через Core
EnvironmentReport report = EnvironmentInfo.Collect();

// Форматуємо вивід
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("MyProject - інформація про середовище");
Console.WriteLine("Студент: Новосельська Уляна Василівна, група ФЕІ-35");
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine();

Console.WriteLine($"{"ОС",-30}: {report.OsDescription}");
Console.WriteLine($"{"Runtime",-30}: {report.FrameworkDescription}");
Console.WriteLine($"{"Архітектура",-30}: {report.ProcessArchitecture}");
Console.WriteLine($"{"RID (визначено вручну)",-30}: {report.DetectedRid}");
Console.WriteLine($"{"RID (від .NET)",-30}: {report.ReportedRid}");
Console.WriteLine($"{"Каталог застосунку",-30}: {report.BaseDirectory}");
Console.WriteLine($"{"Build",-30}: {report.BuildNote}");

Console.WriteLine();
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
Console.WriteLine("Призначення: облік видач примірників книг читачам та їх повернень");
Console.WriteLine("=".PadRight(60, '='));