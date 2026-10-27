using Core.Domain;

namespace Core.Abstractions;

public interface IBookStore
{
    IReadOnlyList<BookCopy> List();//повертає всі книги (IReadOnlyList, тільки читання).
    BookCopy? GetById(string id);//шукає одну книгу, може повернути null (nullable).
    void Add(BookCopy item);// додати
    void Update(BookCopy item);//оновити стан.
    bool Remove(string id);//видалити 
    
    //пошук за предикатом
    IReadOnlyList<BookCopy> Find(Func<BookCopy, bool> predicate);
}