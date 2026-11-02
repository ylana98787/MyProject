using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<BookCopy> Books()
    {
        // Книги 1840-1928 років
        yield return BookCopy.Create("BC-001", "978-966-03-4321-5", "Кобзар", 1840);
        yield return BookCopy.Create("BC-002", "978-966-03-1234-7", "Тіні забутих предків", 1911);
        yield return BookCopy.Create("BC-003", "978-966-03-5678-9", "Місто", 1928);
        yield return BookCopy.Create("BC-004", "978-966-03-1111-2", "Марко Проклятий", 1870);
        yield return BookCopy.Create("BC-005", "978-966-03-2222-3", "Хіба ревуть воли", 1875);
        yield return BookCopy.Create("BC-006", "978-966-03-3333-4", "Intermezzo", 1908);
        yield return BookCopy.Create("BC-007", "978-966-03-4444-5", "Лісова пісня", 1911);
        yield return BookCopy.Create("BC-008", "978-966-03-5555-6", "Захар Беркут", 1883);
        yield return BookCopy.Create("BC-009", "978-966-03-6666-7", "Камінний хрест", 1900);
        yield return BookCopy.Create("BC-010", "978-966-03-7777-8", "Бояриня", 1910);
        
        // Давні видання (1450-1451 роки)
        yield return BookCopy.Create("BC-011", "978-966-04-1111-1", "Слово о полку Ігоревім", 1450);
        yield return BookCopy.Create("BC-012", "978-966-04-2222-2", "Повість минулих літ", 1451);
        
        // Книги 1798-1914 років
        yield return BookCopy.Create("BC-013", "978-966-04-3333-3", "Енеїда", 1798);
        yield return BookCopy.Create("BC-014", "978-966-04-4444-4", "Наталка Полтавка", 1819);
        yield return BookCopy.Create("BC-015", "978-966-04-5555-5", "Чорна рада", 1857);
        yield return BookCopy.Create("BC-016", "978-966-04-6666-6", "Кайдашева сім'я", 1879);
        yield return BookCopy.Create("BC-017", "978-966-04-7777-7", "Сон", 1914);
    }
}