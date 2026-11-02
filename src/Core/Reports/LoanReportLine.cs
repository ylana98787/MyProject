namespace Core.Reports;

public sealed record LoanReportLine(
    string CopyId,
    string Title,
    string ReaderId,
    DateTime IssuedOn);