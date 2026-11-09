using Core.Domain;

namespace Core.Abstractions;

public interface IAsyncBookImporter
{
    IAsyncEnumerable<BookCopy> LoadStreamAsync(
        string path,
        CancellationToken ct = default);
}