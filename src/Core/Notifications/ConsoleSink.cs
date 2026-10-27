using Core.Domain;

namespace Core.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Send(LibraryChangedEventArgs e)
    {
        var line = $"[ConsoleSink] {e.At:HH:mm:ss} {e.Kind,-14} {e.CopyId}";
        Console.WriteLine(line);
    }
}