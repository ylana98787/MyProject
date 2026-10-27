using Core.Domain;

namespace Core.Notifications;

public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = Path.GetFullPath(path);

    public void Send(LibraryChangedEventArgs e)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var line = $"[FileSink] {e.At:HH:mm:ss} {e.Kind,-14} {e.CopyId}";
        File.AppendAllText(_path, line + Environment.NewLine);
    }
}