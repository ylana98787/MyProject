namespace Core.Domain;

/// <summary>
/// Сервіс, який координує операції над кількома сутностями.
/// Інваріанти, що охоплюють ДВІ сутності, живуть тут, а не в сутностях.
/// </summary>
public sealed class LoanService
{
    private const int MaxActiveLoansPerReader = 5;
    // Приватне поле — колекція видач. "readonly" — посилання не можна замінити.
    // List<T> має Add/Remove/Clear — але тільки для внутрішнього використання!
    private readonly List<Loan> _loans = [];//Приватне поле	

// Публічна властивість — тільки для читання.
    // AsReadOnly() — обгортка над _loans без копіювання.
    // Ззовні немає методів Add/Clear — обійти перевірки неможливо.
    public IReadOnlyList<Loan> Loans => _loans.AsReadOnly();

    /// <summary>
    /// Відкрити видачу: перевіряє ліміт активних видач у читача і створює Loan.
    /// </summary>
    public Loan OpenLoan(string loanId, BookCopy copy, string readerId, DateTime issuedOn)//Методи зміни
    {
        // Інваріант, що охоплює ДВІ сутності: читач + його видачі
        int activeCount = _loans.Count(l => l.ReaderId == readerId && !l.IsClosed);

        if (activeCount >= MaxActiveLoansPerReader)
            throw new InvalidOperationException(
                $"Читач {readerId} вже має {activeCount} активних видач (максимум {MaxActiveLoansPerReader}), нову видачу відкрити неможливо");

        var loan = Loan.Open(loanId, copy, readerId, issuedOn);
        _loans.Add(loan); // тут викликається Add
        return loan;
    }

    /// <summary>
    /// Закрити видачу за Id.
    /// </summary>
    public void CloseLoan(string loanId, DateTime returnedOn)//Методи зміни
    {
        var loan = _loans.FirstOrDefault(l => l.Id == loanId)
            ?? throw new InvalidOperationException($"Видачу {loanId} не знайдено");

        loan.Close(returnedOn);
    }
}