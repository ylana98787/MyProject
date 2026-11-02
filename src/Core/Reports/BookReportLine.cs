namespace Core.Reports;

public sealed record BookReportLine(
    string Id,
    string Title,
    string Isbn,
    int Year,
    string Status);