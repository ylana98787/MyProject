using Core.Domain;

namespace Core.Tests;

public class BookCopyTests
{
    [Fact]
    public void Issue_WhenAlreadyIssued_Throws_AndKeepsState()
    {
        // Arrange
        var copy = BookCopy.Create("T-001", "111-111", "Книга", 1900);
        copy.Issue();

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => copy.Issue());

        // Стан не змінився
        Assert.Equal(CopyStatus.Issued, copy.Status);
    }
}