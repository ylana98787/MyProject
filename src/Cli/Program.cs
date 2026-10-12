using System.Text;
using Core.Domain;

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