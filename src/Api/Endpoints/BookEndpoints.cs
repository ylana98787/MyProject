using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBooks(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // GET: Отримати всі книги
        group.MapGet("/", (IBookStore store) =>
        {
            var books = store.List();
            return TypedResults.Ok(books.Select(b => new BookDto(b.Id, b.Isbn, b.Title, b.Year, b.IsIssued)));
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
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var book = BookCopy.Create(id, request.Isbn, request.Title, request.Year);
            store.Add(book);
            
            var dto = new BookDto(book.Id, book.Isbn, book.Title, book.Year, book.IsIssued);
            return TypedResults.Created($"/api/books/{book.Id}", dto);
        });

        // PUT: Оновити книгу
        group.MapPut("/{id}", Results<Ok<BookDto>, NotFound, ValidationProblem> (string id, SaveBookRequest request, IBookStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var existing = store.GetById(id);
            if (existing is null) return TypedResults.NotFound();

            var updated = BookCopy.Create(id, request.Isbn, request.Title, request.Year);
            store.Update(updated);

            return TypedResults.Ok(new BookDto(updated.Id, updated.Isbn, updated.Title, updated.Year, updated.IsIssued));
        });

        // DELETE: Видалити книгу
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, IBookStore store) =>
        {
            var removed = store.Remove(id);
            return removed ? TypedResults.NoContent() : TypedResults.NotFound();
        });
    }
}