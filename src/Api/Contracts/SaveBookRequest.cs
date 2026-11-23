namespace Api.Contracts;

public sealed record SaveBookRequest(string Isbn, string Title, int Year);
