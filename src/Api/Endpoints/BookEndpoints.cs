using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Filters;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBooks(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // GET: Отримати всі книги (з пагінацією та пошуком)
        group.MapGet("/", (int? page, int? pageSize, string? search, IBookStore store) =>
        {
            var pageVal = page ?? 1;
            var pageSizeVal = pageSize ?? 20;

            if (pageVal < 1) pageVal = 1;
            if (pageSizeVal < 1) pageSizeVal = 20;

            var query = store.List().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => 
                    b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    b.Isbn.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var total = query.Count();
            
            var items = query
                .Skip((pageVal - 1) * pageSizeVal)
                .Take(pageSizeVal)
                .Select(b => new BookDto(b.Id, b.Isbn, b.Title, b.Year, b.IsIssued))
                .ToList();

            return TypedResults.Ok(new PagedResult<BookDto>(items, total));
        });

        // GET: Отримати книгу за id
        group.MapGet("/{id}", Results<Ok<BookDto>, NotFound> (string id, IBookStore store) =>
        {
            var book = store.GetById(id);
            return book is not null 
                ? TypedResults.Ok(new BookDto(book.Id, book.Isbn, book.Title, book.Year, book.IsIssued))
                : TypedResults.NotFound();
        });

        // POST: Створити нову книгу
        group.MapPost("/", Results<Created<BookDto>, ValidationProblem> (SaveBookRequest request, IBookStore store) =>
        {
            // Перевірка на реальний дублікат ISBN
            var existingBook = store.List().FirstOrDefault(b => b.Isbn == request.Isbn);
            if (existingBook != null) 
                throw new InvalidOperationException($"Книга з ISBN {request.Isbn} вже існує.");

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var book = BookCopy.Create(id, request.Isbn, request.Title, request.Year);
            store.Add(book);
            
            var dto = new BookDto(book.Id, book.Isbn, book.Title, book.Year, book.IsIssued);
            return TypedResults.Created($"/api/books/{book.Id}", dto);
        })
        .AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

        // PUT: Оновити книгу
        group.MapPut("/{id}", Results<Ok<BookDto>, NotFound, ValidationProblem> (string id, SaveBookRequest request, IBookStore store) =>
        {
            var existing = store.GetById(id);
            if (existing is null) return TypedResults.NotFound();

            var updated = BookCopy.Create(id, request.Isbn, request.Title, request.Year);
            store.Update(updated);

            return TypedResults.Ok(new BookDto(updated.Id, updated.Isbn, updated.Title, updated.Year, updated.IsIssued));
        })
        .AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

        // DELETE: Видалити книгу
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IBookStore store) =>
        {
            var removed = store.Remove(id);
            return removed ? TypedResults.NoContent() : TypedResults.NotFound();
        });
    }
}