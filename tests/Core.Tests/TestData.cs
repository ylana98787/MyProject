using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static List<BookCopy> Items()
    {
        return new List<BookCopy>
        {
            BookCopy.Create("T-001", "111-111", "Книга 1", 1840),
            BookCopy.Create("T-002", "222-222", "Книга 2", 1900),
            BookCopy.Create("T-003", "333-333", "Книга 3", 1928),
            BookCopy.Create("T-004", "444-444", "Книга 4", 1870),
        };
    }
}