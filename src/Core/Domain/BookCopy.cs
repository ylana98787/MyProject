using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    private bool _isIssued;

    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public int Year { get; }
    public bool IsIssued => _isIssued;

    // Приватний конструктор — створити можна лише через Create
    private BookCopy(string id, string isbn, string title, int year)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Year = year;
    }

    // Фабричний метод — єдина точка створення
    public static BookCopy Create(string id, string isbn, string title, int year)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва не може бути порожньою", nameof(title));

        if (year < 1450 || year > DateTime.Now.Year)
            throw new ArgumentOutOfRangeException(nameof(year), year,
                $"Рік {year} поза допустимими межами (1450...{DateTime.Now.Year})");

        return new BookCopy(id.Trim(), isbn.Trim(), title.Trim(), year);
    }

    // Видати примірник
    public void Issue()
    {
        if (_isIssued)
            throw new InvalidOperationException(
                $"Примірник {Id} вже виданий, повторна видача неможлива");

        _isIssued = true;
    }

    // Повернути примірник
    public void Return()
    {
        if (!_isIssued)
            throw new InvalidOperationException(
                $"Примірник {Id} не був виданий, повернення неможливе");

        _isIssued = false;
    }

    // Мапінг у DTO і назад
    public BookDto ToDto() => new(Id, Isbn, Title, Year, IsIssued);

    public static BookCopy FromDto(BookDto dto)
    {
        var copy = Create(dto.Id, dto.Isbn, dto.Title, dto.Year);
        if (dto.IsIssued)
            copy.Issue();
        return copy;
    }

    public override string ToString() =>
        $"{Id} [ISBN: {Isbn}] {Title} ({Year}) — {(IsIssued ? "видано" : "доступно")}";
}