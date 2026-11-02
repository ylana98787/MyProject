namespace Core.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Повертає топ-N елементів, відсортованих за спаданням ключа.
    /// </summary>
    public static IReadOnlyList<T> TopBy<T>(
        this IEnumerable<T> source,
        Func<T, int> keySelector,
        int n)
        => source
            .OrderByDescending(keySelector)
            .Take(n)
            .ToList();
}