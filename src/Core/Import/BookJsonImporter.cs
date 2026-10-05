using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var deserialized = JsonSerializer.Deserialize<List<BookDto>>(json, Options) ?? [];

            for (int i = 0; i < deserialized.Count; i++)
            {
                var book = deserialized[i];
                int number = i + 1;

                // Перевірка: порожні обов'язкові поля
                if (string.IsNullOrWhiteSpace(book.Id) ||
                    string.IsNullOrWhiteSpace(book.Isbn) ||
                    string.IsNullOrWhiteSpace(book.Title))
                {
                    errors.Add($"запис {number}: ISBN або назва порожні");
                    continue;
                }

                // Перевірка року
                if (book.Year < 1450 || book.Year > DateTime.Now.Year)
                {
                    errors.Add($"запис {number}: рік '{book.Year}' поза допустимими межами");
                    continue;
                }

                items.Add(book);
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"помилка розбору JSON: {ex.Message}");
        }

        return new ImportResult<BookDto>(items, errors);
    }
}