namespace Core.Domain;

public sealed class Loan
{
    private DateTime? _returnedOn;

    public string Id { get; }
    public BookCopy Copy { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn => _returnedOn;

    public bool IsClosed => _returnedOn.HasValue;

    // Приватний конструктор
    private Loan(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        Id = id;
        Copy = copy;
        ReaderId = readerId;
        IssuedOn = issuedOn;
    }

    // Фабричний метод — відкрити видачу
    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));

        if (copy is null)
            throw new ArgumentNullException(nameof(copy), "Примірник обов'язковий");

        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        // Викликаємо Issue на примірнику — він сам перевірить, чи не виданий вже
        copy.Issue();

        return new Loan(id.Trim(), copy, readerId.Trim(), issuedOn);
    }

    // Закрити видачу — повернути примірник
    public void Close(DateTime returnedOn)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Видача {Id} вже закрита {_returnedOn:yyyy-MM-dd}, повторне закриття неможливе");

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn,
                $"Дата повернення ({returnedOn:yyyy-MM-dd}) не може бути раніше дати видачі ({IssuedOn:yyyy-MM-dd})");

        _returnedOn = returnedOn;
        Copy.Return();
    }

    public override string ToString()
    {
        string status = IsClosed
            ? $"повернено {_returnedOn:yyyy-MM-dd}"
            : "активна";
        return $"{Id} — {Copy.Id} для {ReaderId}, видано {IssuedOn:yyyy-MM-dd}, {status}";
    }
}