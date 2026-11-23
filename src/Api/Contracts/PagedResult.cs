namespace Api.Contracts;

public sealed record PagedResult<T>(IEnumerable<T> Items, int Total);
