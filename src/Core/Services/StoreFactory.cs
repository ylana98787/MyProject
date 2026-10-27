using Core.Abstractions;
using Core.Storage;

namespace Core.Services;

public static class StoreFactory
{
    public static IBookStore Create(string[] args, string dataPath)
    {
        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache"); // ← додаємо перевірку на кеш

        // 1. Створюємо базове сховище
        IBookStore baseStore = useFile
            ? new FileBookStore(dataPath)
            : new InMemoryBookStore(SampleData.Books());

        // 2. Якщо є аргумент --cache, обгортаємо в декоратор, інакше повертаємо як є
        return useCache
            ? new CachingBookStore(baseStore)
            : baseStore;
    }
}