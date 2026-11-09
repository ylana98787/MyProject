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
        // Створюємо порожнє сховище в пам'яті (без файлів, без диска).
    // Воно потрібне, щоб сервіс міг кудись додавати книгу.
        var store = new InMemoryBookStore();
        // Створюємо порожнє сховище в пам'яті (без файлів, без диска).
    // Воно потрібне, щоб сервіс міг кудись додавати книгу.
        var service = new LendingService(store);
       / / Створюємо порожній список, куди будемо складати всі події, що виникають.
    // Це "записничок" для перевірки.
        var events = new List<LibraryChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);//Підписався на подію Changed:
        // кожен раз, коли подія виникає — додаю її в список.

        // Act
        service.AddBook("111-111", "Нова книга", 2024);// це має викликати подію рівно один раз.

        // Assert
        Assert.Single(events);//Перевірив через Assert.Single(events), що в списку рівно один елемент.
        Assert.Equal(LibraryChangeKind.BookAdded, events[0].Kind);//Перевірив, що це саме BookAdded
    }
}