using Core.Storage;

namespace Core.Tests;

public class JsonBookImporterTests
{
    [Fact]
    public async Task LoadAsync_WithInvalidData_ThrowsDomainException()
    {
        // Arrange
        var tmp = Path.GetTempFileName();
        await File.WriteAllTextAsync(tmp, """
            [
              { "id": "", "isbn": "111", "title": "Погана книга", "year": 1900 }
            ]
            """);

        try
        {
            var importer = new JsonBookImporter();

            // Act + Assert
            await Assert.ThrowsAsync<DomainException>(
                () => importer.LoadAsync(tmp, CancellationToken.None));
        }
        finally
        {
            File.Delete(tmp);
        }
    }
}