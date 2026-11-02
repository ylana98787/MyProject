using Core.Domain;
using Core.Extensions;

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтр + сортування + проєкція
    // Питання: "Які книги мають найменше... / старіші за N років?"
    public static IReadOnlyList<BookReportLine> OldBooks(IEnumerable<BookCopy> source, int beforeYear)
        => source.Where(b => b.Year < beforeYear)
                 .OrderBy(b => b.Year).ThenBy(b => b.Id, StringComparer.Ordinal)
                 .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
                 .ToList();

    // 2) Топ-N зі стабільним порядком
    // Питання: "Які найстаріші N книг?"
    public static IReadOnlyList<BookReportLine> TopOldest(IEnumerable<BookCopy> source, int n)
        => source.OrderBy(b => b.Year).ThenBy(b => b.Id, StringComparer.Ordinal)
                 .Take(n)
                 .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
                 .ToList();

    // 3) Групування з підрахунком
    // Питання: "Скільки книг у кожному статусі (Available/Issued)?"
    public static IReadOnlyList<GroupSummary> ByStatus(IEnumerable<BookCopy> source)
        => source.GroupBy(b => b.Status.ToString(), StringComparer.OrdinalIgnoreCase)
                 .Select(g => new GroupSummary(g.Key, g.Count(), g.Count()))
                 .OrderByDescending(s => s.Count)
                 .ToList();

    // 4) Агрегати з обробкою порожньої послідовності
    // Питання: "Скільки всього книг, який середній рік видання, найстаріша?"
    public static (int Items, double AverageYear, int OldestYear, int NewestYear) Totals(IEnumerable<BookCopy> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0d, 0, 0)
            : (list.Count,
               list.Average(b => b.Year),
               list.Min(b => b.Year),
               list.Max(b => b.Year));
    }

    // 5) Пошук за предикатом (для FindAll/демонстрації Func)
    public static IReadOnlyList<BookReportLine> Search(IEnumerable<BookCopy> source, Func<BookCopy, bool> predicate)
        => source.Where(predicate)
                 .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
                 .ToList();

                 // 6) Join двох колекцій
               // Питання: "Які книги видані яким читачам?"
  public static IReadOnlyList<LoanReportLine> JoinedLoans(
    IEnumerable<BookCopy> copies,
    IEnumerable<Loan> loans)
    => copies
        .Join(loans,
              copy => copy.Id,
              loan => loan.Copy.Id,
              (copy, loan) => new LoanReportLine(
                  copy.Id,
                  copy.Title,
                  loan.ReaderId,
                  loan.IssuedOn))
        .OrderBy(l => l.IssuedOn)
        .ThenBy(l => l.CopyId, StringComparer.Ordinal)
        .ToList();

        // 7) Топ-5 найновіших (доп. 2 — TopBy)
public static IReadOnlyList<BookReportLine> TopNewest(IEnumerable<BookCopy> source, int n)
    => source.TopBy(b => b.Year, n)
             .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
             .ToList();

// 8) Топ-N за кількістю символів у назві (доп. 2 — TopBy)
public static IReadOnlyList<BookReportLine> TopByLongestTitle(IEnumerable<BookCopy> source, int n)
    => source.TopBy(b => b.Title.Length, n)
             .Select(b => new BookReportLine(b.Id, b.Title, b.Isbn, b.Year, b.Status.ToString()))
             .ToList();

             // 9) Синтаксис запитів (доп. 3) — той самий звіт, що ByStatus, але через from/group/into
public static IReadOnlyList<GroupSummary> ByStatusQuerySyntax(IEnumerable<BookCopy> source)
    => (from book in source
        group book by book.Status.ToString() into g
        orderby g.Count() descending
        select new GroupSummary(g.Key, g.Count(), g.Count()))
       .ToList();
       
}