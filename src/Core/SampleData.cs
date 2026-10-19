using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<BookCopy> Books()
    {
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
    }
}