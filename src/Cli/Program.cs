using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

// ===== СЦЕНАРІЙ 1: УСПІХ =====
Console.WriteLine("=== Сценарій 1: успіх ===");

var copy = BookCopy.Create("BC-001", "978-966-03-4321-5", "Кобзар", 1840);
Console.WriteLine(copy);

var loan = Loan.Open("L-001", copy, "R-001", new DateTime(2026, 10, 1));
Console.WriteLine(loan);
Console.WriteLine(copy);

loan.Close(new DateTime(2026, 10, 15));
Console.WriteLine(loan);
Console.WriteLine(copy);

Console.WriteLine();

// ===== СЦЕНАРІЙ 2: ПОРУШЕННЯ ІНВАРІАНТІВ =====
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

var copy2 = BookCopy.Create("BC-002", "978-966-03-1111-2", "Місто", 1928);
copy2.Issue();   // тепер видано

TryDo("видати вже виданий примірник", () => copy2.Issue());
TryDo("порожній ISBN", () => BookCopy.Create("BC-003", "", "Тіні забутих предків", 1911));
TryDo("порожня назва", () => BookCopy.Create("BC-004", "978-966-03-2222-3", " ", 1875));
TryDo("рік поза межами", () => BookCopy.Create("BC-005", "978-966-03-3333-4", "Intermezzo", 3000));
TryDo("повернути невиданий примірник", () =>
{
    var c = BookCopy.Create("BC-006", "978-966-03-4444-5", "Лісова пісня", 1911);
    c.Return();
});

var copy3 = BookCopy.Create("BC-007", "978-966-03-5555-6", "Захар Беркут", 1883);
var loan3 = Loan.Open("L-003", copy3, "R-003", new DateTime(2026, 10, 10));
loan3.Close(new DateTime(2026, 10, 20));
TryDo("закрити вже закриту видачу", () => loan3.Close(new DateTime(2026, 10, 25)));

TryDo("дата повернення раніше видачі", () =>
{
    var c = BookCopy.Create("BC-008", "978-966-03-6666-7", "Камінний хрест", 1900);
    var l = Loan.Open("L-004", c, "R-004", new DateTime(2026, 10, 20));
    l.Close(new DateTime(2026, 10, 10));
});

Console.WriteLine();
Console.WriteLine("=== Стан після всіх помилок ===");
Console.WriteLine(copy2);   // має залишитися "видано"
Console.WriteLine(loan3);   // має залишитися "повернено 2026-10-20"

Console.WriteLine();
Console.WriteLine("=== Мапінг ToDto / FromDto ===");
var original = BookCopy.Create("BC-100", "978-966-03-9999-9", "Тест", 2000);
original.Issue();
var dto = original.ToDto();
Console.WriteLine($"ToDto:   {dto}");
var restored = BookCopy.FromDto(dto);
Console.WriteLine($"FromDto: {restored}");
Console.WriteLine($"Стан збережено: {restored.IsIssued == original.IsIssued}");
Console.WriteLine();
Console.WriteLine("=== Сценарій 3: імпорт → сутності ===");

// Імпортуємо CSV з тижня 3
var importResult = BookCsvImporter.Load(Path.Combine("data", "sample.csv"));

// Перетворюємо DTO → сутності (з перевіркою інваріантів)
var domainResult = BookCopy.FromImportResult(importResult);

Console.WriteLine($"Успішно створено сутностей: {domainResult.Items.Count}");
foreach (var domainCopy in domainResult.Items.Take(5))
{
    Console.WriteLine($"  {domainCopy}");
}

Console.WriteLine();
Console.WriteLine($"Помилок (парсер + інваріанти): {domainResult.Errors.Count}");
foreach (var err in domainResult.Errors)
{
    Console.WriteLine($"  ! {err}");
}

// Перевірка: чи інваріанти справді захищають?
Console.WriteLine();
Console.WriteLine("Перевірка: створюємо BookCopy з поганого DTO");
TryDo("рік 3000 (порушує інваріант)", () =>
{
    var badDto = new BookDto("BC-999", "978-966-03-9999-9", "Майбутнє", 3000);
    BookCopy.FromDto(badDto);
});
Console.WriteLine();
Console.WriteLine("=== Сценарій 4: інваріант, що охоплює дві сутності ===");

var service = new LoanService();

// Створюємо 5 активних видач для читача R-100
for (int i = 1; i <= 5; i++)
{
    var c = BookCopy.Create($"BC-{100 + i}", $"978-966-11-000{i}-1", $"Книга {i}", 2000);
    service.OpenLoan($"L-{200 + i}", c, "R-100", new DateTime(2026, 10, 1));
}
Console.WriteLine($"Створено активних видач для R-100: {service.Loans.Count}");

// Спроба створити шосту видачу — має впасти
TryDo("шостий активний Loan (перевищено ліміт)", () =>
{
    var extraCopy = BookCopy.Create("BC-200", "978-966-11-9999-9", "Зайва книга", 2020);
    service.OpenLoan("L-300", extraCopy, "R-100", new DateTime(2026, 10, 1));
});

// Але для іншого читача — можна
var otherCopy = BookCopy.Create("BC-201", "978-966-11-8888-8", "Інша книга", 2020);
var otherLoan = service.OpenLoan("L-301", otherCopy, "R-200", new DateTime(2026, 10, 1));
Console.WriteLine($"Для R-200 видача відкрита: {otherLoan}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 5: enum-стани і переходи ===");

var c1 = BookCopy.Create("BC-300", "978-966-12-0001-1", "Тестова", 2020);
Console.WriteLine($"Початковий стан: {c1.Status}");

c1.Issue();
Console.WriteLine($"Після Issue: {c1.Status}");

c1.Return();
Console.WriteLine($"Після Return: {c1.Status}");

c1.MarkDamaged();
Console.WriteLine($"Після MarkDamaged: {c1.Status}");

// Заборонені переходи
TryDo("Lost → Issued (заборонено)", () =>
{
    var c = BookCopy.Create("BC-301", "978-966-12-0002-2", "Втрачена", 2020);
    c.MarkLost();
    c.Issue();  // ← має впасти
});

TryDo("Damaged → Issued (заборонено)", () =>
{
    var c = BookCopy.Create("BC-302", "978-966-12-0003-3", "Пошкоджена", 2020);
    c.MarkDamaged();
    c.Issue();  // ← має впасти
});

// ===== ДОПОМІЖНА ФУНКЦІЯ =====
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}