using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Api.Contracts;
using Microsoft.AspNetCore.Hosting;

namespace Api.Tests;

public class BooksApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BooksApiTests(WebApplicationFactory<Program> factory)
{
    _client = factory.WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Testing");
    }).CreateClient();
}

    [Fact]
    public async Task PostBook_ReturnsCreatedAndLocation_ThenGetReturnsOk()
    {
        // 1. Arrange (Підготовка даних)
        var request = new SaveBookRequest("ISBN-TEST-999", "Integration Test Book", 2024);

        // 2. Act (POST)
        var postResponse = await _client.PostAsJsonAsync("/api/books", request);

        // 3. Assert (Перевірка POST)
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);
        
        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(createdBook);
        Assert.Equal("ISBN-TEST-999", createdBook.Isbn);

        // 4. Act (GET за Location)
        var getResponse = await _client.GetAsync(postResponse.Headers.Location);

        // 5. Assert (Перевірка GET)
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var fetchedBook = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(fetchedBook);
        Assert.Equal(createdBook.Id, fetchedBook.Id);
    }
}
