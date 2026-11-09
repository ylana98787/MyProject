using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonBookStreamImporter : IAsyncBookImporter
{
    public async IAsyncEnumerable<BookCopy> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Асинхронно десеріалізуємо список DTO (JSON-парсер не підтримує streaming для масиву)
        var dtos = await JsonSerializer.DeserializeAsync<List<BookDto>>(
            stream, options, cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        // Віддаємо по одному елементу через yield return
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            BookCopy copy;
            try
            {
                copy = BookCopy.FromDto(dtos[i]);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }

            yield return copy;
        }
    }
}