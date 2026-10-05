using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    // Результат може містити або книги, або читачів
    public static ImportResult<object> Load(string path)
    {
        var items = new List<object>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Загальні перевірки
            { Length: < 5 } => new ParseFailed($"очікую 5 колонок, отримав {parts.Length}"),

            // Книга: type = "B"
            ["B", var id, var isbn, var title, var year]
                when !string.IsNullOrWhiteSpace(id)
                  && !string.IsNullOrWhiteSpace(isbn)
                  && !string.IsNullOrWhiteSpace(title)
                  && int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out int y)
                  && y >= 1450 && y <= DateTime.Now.Year
                => new ParseOk(new BookDto(id, isbn, title, y)),

            // Читач: type = "R"
            ["R", var id, _, var fullName, _]
                when !string.IsNullOrWhiteSpace(id)
                  && !string.IsNullOrWhiteSpace(fullName)
                => new ParseOk(new ReaderDto(id, fullName, $"{id.ToLowerInvariant()}@library.local")),

            // Невідомий тип
            [var type, ..] => new ParseFailed($"невідомий тип запису: '{type}'"),

            _ => new ParseFailed("не вдалося розібрати рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(object Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}