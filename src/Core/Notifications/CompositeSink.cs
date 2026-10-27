using Core.Domain;

namespace Core.Notifications;

public sealed class CompositeSink(params INotificationSink[] sinks) : INotificationSink
{
    private readonly INotificationSink[] _sinks = sinks;

    public void Send(LibraryChangedEventArgs e)
    {
        foreach (var sink in _sinks)
            sink.Send(e);
    }
}