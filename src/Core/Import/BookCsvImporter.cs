using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class BookCsvImporter
{
    // Роздільник — крапка з комою: не конфліктує з комою в назвах
    private const char Separator = ';';

    public static ImportResult<BookDto> Load(string path, Action<string>? onError = null)
{
          var items = new List<BookDto>();
         var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            // Порожні рядки та коментарі — пропускаємо
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Рядок заголовків — пропускаємо
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                   case ParseFailed failed:
                      var errorMsg = $"рядок {number}: {failed.Reason}";
                      errors.Add(errorMsg);
                     onError?.Invoke(errorMsg);   // ← викликаємо делегат, якщо він переданий
                      break;
            }

        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 } => new ParseFailed($"очікую 4 колонки, отримав {parts.Length}"),
            [_, "", _, _] or [_, _, "", _] => new ParseFailed("ISBN або назва порожні"),
            [_, _, _, var year] when !int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) || y < 1450 || y > DateTime.Now.Year
                => new ParseFailed($"рік '{year}' поза допустимими межами"),
            [var id, var isbn, var title, var year] when int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y)
                => new ParseOk(new BookDto(id, isbn, title, y)),
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(BookDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}