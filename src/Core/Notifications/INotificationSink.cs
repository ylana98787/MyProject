using Core.Domain;

namespace Core.Notifications;

public interface INotificationSink
{
    void Send(LibraryChangedEventArgs e);
}