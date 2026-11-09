using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class LendingServiceTests
{
    [Fact]
    public void AddBook_RaisesChangedExactlyOnce()
    {
        // Arrange
        var store = new InMemoryBookStore();
        var service = new LendingService(store);
        var events = new List<LibraryChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);

        // Act
        service.AddBook("111-111", "Нова книга", 2024);

        // Assert
        Assert.Single(events);
        Assert.Equal(LibraryChangeKind.BookAdded, events[0].Kind);
    }
}