using Api.Endpoints;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;
using Api.Contracts;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// 1. Додаємо OpenAPI та ProblemDetails (для гарних помилок)
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); 
builder.Services.AddScoped<IValidator<SaveBookRequest>, SaveBookRequestValidator>();
builder.Services.AddExceptionHandler<Api.Errors.DomainExceptionHandler>();

// 2. Реєстрація сховища в DI (Singleton).
// Створимо окрему папку для API, щоб не змішувати з даними Cli
var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
Directory.CreateDirectory(dataDir); // Створюємо папку, якщо її немає
var dataPath = Path.Combine(dataDir, "api_books.json");
if (builder.Environment.IsEnvironment("Testing"))
{
    // Для інтеграційних тестів використовуємо пам'ять, щоб не залежати від файлів
    builder.Services.AddSingleton<IBookStore, InMemoryBookStore>();
}
else
{
    builder.Services.AddSingleton<IBookStore>(sp => new FileBookStore(dataPath));
}

var app = builder.Build();

// 3. Обробка помилок (щоб клієнт не бачив Stack Trace)
app.UseExceptionHandler();
app.UseStatusCodePages();

// 4. OpenAPI та Scalar лише для Development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 5. Хелсчек (перевірка, що сервер живий)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// 6. Підключаємо наші маршрути для книг
app.MapBooks();

app.Run();public partial class Program { }
