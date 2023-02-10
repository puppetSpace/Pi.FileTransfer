﻿using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Files.Events;
public class FileUpdatedEvent : INotification
{
    public FileUpdatedEvent(File file, Folder folder)
    {
        File = file;
        Folder = folder;
    }
    public File File { get; }

    public Folder Folder { get; }

    public class FileUpdatedEventHandler : FileHandlingBase, INotificationHandler<FileUpdatedEvent>
    {
        private readonly DeltaService _deltaService;
        private readonly DeltaSegmentation _deltaSegmentation;
        private readonly ILogger<FileUpdatedEvent> _logger;

        public FileUpdatedEventHandler(DeltaService deltaService, DeltaSegmentation deltaSegmentation
            , TransferService transferService, DataStore dataStore, ILogger<FileUpdatedEvent> logger)
            : base(logger, transferService, dataStore, true)
        {
            _deltaService = deltaService;
            _deltaSegmentation = deltaSegmentation;
            _logger = logger;
        }

        public async Task Handle(FileUpdatedEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.Folder.Destinations.Any())
                return;

            _logger.ProcessUpdatedFile(notification.File.RelativePath);

            foreach (var destination in notification.Folder.Destinations)
            {
                DataStore.ClearLastPosition(destination, notification.Folder, notification.File);
            }

            await _deltaService.CreateDelta(notification.Folder, notification.File);
            _deltaService.CreateSignature(notification.Folder, notification.File);

            var totalAmountOfSegments = await _deltaSegmentation.Segment(notification.Folder, notification.File, SendSegment);
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
