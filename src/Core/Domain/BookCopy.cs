using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    private CopyStatus _status;

    public string Id { get; }
    public string Isbn { get; }
    public string Title { get; }
    public int Year { get; }
    public CopyStatus Status => _status;
    public bool IsIssued => _status == CopyStatus.Issued;

    // Приватний конструктор
    private BookCopy(string id, string isbn, string title, int year, CopyStatus status)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Year = year;
        _status = status;
    }

    // Фабричний метод
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

        return new BookCopy(id.Trim(), isbn.Trim(), title.Trim(), year, CopyStatus.Available);
    }

    // Видати примірник
    public void Issue() => ChangeStatus(CopyStatus.Issued);

    // Повернути примірник
    public void Return() => ChangeStatus(CopyStatus.Available);

    // Позначити як втрачений
    public void MarkLost() => ChangeStatus(CopyStatus.Lost);

    // Позначити як пошкоджений
    public void MarkDamaged() => ChangeStatus(CopyStatus.Damaged);

    // Перевірка допустимих переходів через switch expression
    private void ChangeStatus(CopyStatus newStatus)
    {
        bool allowed = (_status, newStatus) switch
        {
            // З Available можна: видати, втратити, пошкодити
            (CopyStatus.Available, CopyStatus.Issued) => true,
            (CopyStatus.Available, CopyStatus.Lost) => true,
            (CopyStatus.Available, CopyStatus.Damaged) => true,

            // З Issued можна: повернути, втратити, пошкодити
            (CopyStatus.Issued, CopyStatus.Available) => true,
            (CopyStatus.Issued, CopyStatus.Lost) => true,
            (CopyStatus.Issued, CopyStatus.Damaged) => true,

            // З Lost не можна нічого (крім, наприклад, знайти — не реалізовано)
            // З Damaged можна: полагодити (повернути в Available)
            (CopyStatus.Damaged, CopyStatus.Available) => true,

            // Усі інші переходи — заборонені
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Примірник {Id}: перехід {_status} → {newStatus} заборонений");

        _status = newStatus;
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

    // Імпорт → сутності (додаткове завдання 1)
    public static ImportResult<BookCopy> FromImportResult(ImportResult<BookDto> importResult)
    {
        var copies = new List<BookCopy>();
        var errors = new List<string>(importResult.Errors);

        for (int i = 0; i < importResult.Items.Count; i++)
        {
            var dto = importResult.Items[i];
            int number = i + 1;

            try
            {
                copies.Add(FromDto(dto));
            }
            catch (Exception ex)
            {
                errors.Add($"запис {number} ({dto.Id}): {ex.GetType().Name} — {ex.Message}");
            }
        }

        return new ImportResult<BookCopy>(copies, errors);
    }

    public override string ToString() =>
        $"{Id} [ISBN: {Isbn}] {Title} ({Year}) — {_status}";
}