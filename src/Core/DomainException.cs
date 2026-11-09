namespace Core;

/// <summary>
/// Доменний виняток — для порушень бізнес-правил.
/// Відрізняється від ArgumentException (некоректний аргумент)
/// і від IOException/JsonException (технічні помилки).
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception inner) : base(message, inner) { }
}