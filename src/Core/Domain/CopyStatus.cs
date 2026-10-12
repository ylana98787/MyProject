namespace Core.Domain;

/// <summary>
/// Стан примірника книги.
/// </summary>
public enum CopyStatus
{
    Available,   // доступний для видачі
    Issued,      // видано читачеві
    Lost,        // втрачено
    Damaged      // пошкоджено
}