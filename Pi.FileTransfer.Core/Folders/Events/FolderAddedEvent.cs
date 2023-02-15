using MediatR;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Folders.Events;
public class FolderAddedEvent : INotification
{
    public FolderAddedEvent(Folder folder)
    {
        Folder = folder;
    }
    public Folder Folder { get; }

    internal class FolderAddedEventHandler : INotificationHandler<FolderAddedEvent>
    {
        private readonly FolderState _folderState;

        public FolderAddedEventHandler(FolderState folderState)
        {
            _folderState = folderState;
        }
        public Task Handle(FolderAddedEvent notification, CancellationToken cancellationToken)
        {
            _folderState.AddToState(notification.Folder);
            return Task.CompletedTask;
        }
    }
}
