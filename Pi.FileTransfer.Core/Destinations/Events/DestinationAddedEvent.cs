using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Events;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Events;
public class DestinationAddedEvent : INotification
{
    public DestinationAddedEvent(Folder folder, Destination destination)
    {
        Folder = folder;
        Destination = destination;
    }

    public Folder Folder { get; }
    public Destination Destination { get; }


    public class DestinationAddedEventHandler : FileHandlingBase, INotificationHandler<DestinationAddedEvent>
    {
        private readonly FileSegmentation _fileSegmentation;
        private readonly DeltaService _deltaService;

        public DestinationAddedEventHandler(FileSegmentation fileSegmentation, DeltaService deltaService, ILogger<DestinationAddedEvent> logger, TransferService transferService, IFileTransferRepository fileTransferRepository)
            : base(logger, transferService, fileTransferRepository, false)
        {
            _fileSegmentation = fileSegmentation;
            _deltaService = deltaService;
        }

        public async Task Handle(DestinationAddedEvent notification, CancellationToken cancellationToken)
        {
            Logger.ProcessingAllFilesForDestination(notification.Destination.Name, notification.Folder.Name);
            foreach (var file in notification.Folder.Files)
            {
                try
                {
                    _deltaService.CreateSignature(notification.Folder, file);
                    var totalAmountOfSegments = await _fileSegmentation.Segment(notification.Folder, file, SendSegment);
                    await SendReceipt(notification.Folder, file, totalAmountOfSegments);
                }
                catch (Exception ex)
                {
                    Logger.FailedToProcessFileForDestination(file.GetFullPath(), notification.Destination.Name, ex);
                }
                finally
                {
                    await FileTransferRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }

        private async Task SendReceipt(Folder folder, Files.File file, int totalAmountOfSegments)
        {
            foreach (var destination in folder.Destinations)
            {
                try
                {
                    Logger.SendReceipt(file.RelativePath, destination.Name);
                    await TransferService.SendReceipt(destination, new Receives.Receipt(file.Id, folder.Name, file.RelativePath, totalAmountOfSegments,file.Version, IsFileUpdate));
                }
                catch (Exception ex)
                {
                    Logger.SendReceiptFailed(file.RelativePath, destination.Name, ex);
                    FileTransferRepository.AddFailedReceipt(new(file,destination, totalAmountOfSegments, IsFileUpdate));
                }
            }
        }
    }
}
