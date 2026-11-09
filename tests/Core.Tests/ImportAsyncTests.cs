using Core;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportAsyncTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        // Arrange — створюємо тимчасовий файл, щоб FileNotFound не заважав
        var tmp = Path.GetTempFileName();
        await File.WriteAllTextAsync(tmp, "[]");

        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();   // скасовуємо відразу

            var store = new InMemoryBookStore();
            var service = new LendingService(store);

            // Act + Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportAsync(new JsonBookImporter(), tmp, cts.Token));
        }
        finally
        {
            File.Delete(tmp);
        }
    }
}