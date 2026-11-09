using Core.Reports;

namespace Core.Tests;

public class ReportServiceTests
{
    [Fact]
    public void TopOldest_IsDeterministic()
    {
        // Arrange
        var items = TestData.Items();

        // Act
        var top = ReportService.TopOldest(items, 2);

        // Assert
        Assert.Equal(2, top.Count);
        Assert.Equal("T-001", top[0].Id);   // 1840 — найстаріша
        Assert.Equal("T-004", top[1].Id);   // 1870 — друга
    }

    [Theory]
    [InlineData(1850, 1)]  // тільки T-001 (1840)
    [InlineData(1910, 3)]  // T-001, T-002, T-004
    [InlineData(1900, 2)]  // тільки T-001 (T-002 — не < 1900)
    public void OldBooks_CountsCorrectly(int beforeYear, int expected)
    {
        // Arrange
        var items = TestData.Items();

        // Act
        var result = ReportService.OldBooks(items, beforeYear);

        // Assert
        Assert.Equal(expected, result.Count);
    }
}