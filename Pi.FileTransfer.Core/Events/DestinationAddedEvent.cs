using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Events;
public class DestinationAddedEvent : INotification
{
	public DestinationAddedEvent(Folder folder, Destination destination)
	{
        Folder = folder;
        Destination = destination;
    }

    public Folder Folder { get; }
    public Destination Destination { get; }


    public class DestinationAddedEventHandler : FileHandlingBase,INotificationHandler<DestinationAddedEvent>
    {
        private readonly FileSegmentation _fileSegmentation;
        private readonly DeltaService _deltaService;

        public DestinationAddedEventHandler(FileSegmentation fileSegmentation, DeltaService deltaService, ILogger<DestinationAddedEvent> logger, TransferService transferService, DataStore dataStore) 
            : base(logger, transferService, dataStore, false)
        {
            _fileSegmentation = fileSegmentation;
            _deltaService = deltaService;
        }

        public async Task Handle(DestinationAddedEvent notification, CancellationToken cancellationToken)
        {
            Logger.ProcessingAllFilesForDestination(notification.Destination.Name,notification.Folder.Name);
            foreach (var file in notification.Folder.Files)
            {
                _deltaService.CreateSignature(notification.Folder, file);
                var totalAmountOfSegments = await _fileSegmentation.Segment(notification.Folder, file, SendSegment);
                await SendReceipt(notification.Folder, file, totalAmountOfSegments);
            }
        }

        private async Task SendReceipt(Folder folder, Entities.File file, int totalAmountOfSegments)
        {
            foreach (var destination in folder.Destinations)
            {
                try
                {
                    Logger.SendReceipt(file.RelativePath, destination.Name);
                    await TransferService.SendReceipt(destination, new(file.Id, file.RelativePath, totalAmountOfSegments, folder.Name, IsFileUpdate));
                }
                catch (Exception ex)
                {
                    Logger.SendReceiptFailed(file.RelativePath, destination.Name, ex);
                    await DataStore.StoreFailedReceiptTransfer(destination, folder, file, totalAmountOfSegments, IsFileUpdate);
                }
            }
        }
    }
}
