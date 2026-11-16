using Core.Domain;

namespace Core.Abstractions;

public interface IAsyncBookImporter
{
    IAsyncEnumerable<BookCopy> LoadStreamAsync(//це і є потоковий імпорт
        string path,
        CancellationToken ct = default);
}