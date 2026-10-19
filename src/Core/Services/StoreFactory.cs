using Core.Abstractions;
using Core.Storage;

namespace Core.Services;

public static class StoreFactory
{
    public static IBookStore Create(string[] args, string dataPath)
    {
        bool useFile = args.Contains("--file");

        IBookStore baseStore = useFile
            ? new FileBookStore(dataPath)
            : new InMemoryBookStore(SampleData.Books());

        return new CachingBookStore(baseStore);
    }
}