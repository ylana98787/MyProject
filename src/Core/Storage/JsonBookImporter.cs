using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonBookImporter : IBookImporter
{
    public async Task<IReadOnlyList<BookCopy>> LoadAsync(string path, CancellationToken ct = default)
    {
        // Справжній асинхронний ввід-вивід через FileStream з useAsync: true
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Асинхронна десеріалізація з підтримкою CancellationToken
        var dtos = await JsonSerializer.DeserializeAsync<List<BookDto>>(
            stream,
            options, 
            cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<BookCopy>(dtos.Count);

        for (var i = 0; i < dtos.Count; i++)
        {
            // Перевіряємо скасування на кожній ітерації
            ct.ThrowIfCancellationRequested();

            try
            {
                result.Add(BookCopy.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                // Обгортаємо бізнес-помилку в DomainException із inner exception
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }
}