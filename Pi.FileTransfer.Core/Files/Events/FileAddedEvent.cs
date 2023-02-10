using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;
using System.Collections.Concurrent;

namespace Pi.FileTransfer.Core.Files.Events;
//should actualy be a notification. but is is cleaner to have commands for everything
public class FileAddedEvent : INotification
{
    public FileAddedEvent(File file, Folder folder)
    {
        File = file;
        Folder = folder;
    }

    public File File { get; }

    public Folder Folder { get; }

    public class FileAddedEventhandler : FileHandlingBase, INotificationHandler<FileAddedEvent>
    {
        private readonly FileSegmentation _fileSegmentation;
        private readonly DeltaService _deltaService;

        public FileAddedEventhandler(FileSegmentation fileSegmentation
            , TransferService transferService
            , DataStore transferStore
            , DeltaService deltaService
            , ILogger<FileAddedEvent> logger) : base(logger, transferService, transferStore, false)
        {
            _fileSegmentation = fileSegmentation;
            _deltaService = deltaService;
        }

        public async Task Handle(FileAddedEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.Folder.Destinations.Any())
            {
                return;
            }

            Logger.ProcessNewFile(notification.File.RelativePath);
            _deltaService.CreateSignature(notification.Folder, notification.File);
            var totalAmountOfSegments = await _fileSegmentation.Segment(notification.Folder, notification.File, SendSegment);
            await SendReceipt(notification.Folder, notification.File, totalAmountOfSegments);
        }

        private async Task SendReceipt(Folder folder, File file, int totalAmountOfSegments)
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
