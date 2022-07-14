using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
//should actualy be a notification. but is is cleaner to have commands for everything
public class AddFileCommand : IRequest<Unit>
{
    public AddFileCommand(Entities.File file, Folder folder)
    {
        File = file;
        Folder = folder;
    }

    public Entities.File File { get; }

    public Folder Folder { get; }

    public class AddFileCommandhandler : IRequestHandler<AddFileCommand>
    {
        private readonly FileSegmentation _fileSegmentation;
        private readonly TransferService _transferService;
        private readonly DataStore _transferStore;
        private readonly ILogger<AddFileCommand> _logger;
        private readonly ConcurrentDictionary<string, int> _currentBytesRead = new ConcurrentDictionary<string, int>();

        public AddFileCommandhandler(FileSegmentation fileSegmentation, TransferService transferService, DataStore transferStore, ILogger<AddFileCommand> logger)
        {
            _fileSegmentation = fileSegmentation;
            _transferService = transferService;
            _transferStore = transferStore;
            _logger = logger;
        }

        public async Task<Unit> Handle(AddFileCommand notification, CancellationToken cancellationToken)
        {
            if (!notification.Folder.Destinations.Any())
                return Unit.Value;

            _logger.SegmentFile(notification.File.RelativePath);
            var totalAmountOfSegments = await _fileSegmentation.SegmentFile(notification.Folder, notification.File, SendSegment);
            await SendReceipt(notification.Folder, notification.File, totalAmountOfSegments);

            return Unit.Value;
        }

        private async Task SendSegment(int sequenceNumber, byte[] buffer, Entities.Folder folder, Entities.File file)
        {
            var runningTasks = new List<Task>();
            foreach (var destination in folder.Destinations)
            {
                runningTasks.Add(SendToDestination(sequenceNumber, buffer, folder, file, destination));
            }

            await Task.WhenAll(runningTasks);
            ClearLastPosition(folder, file);
        }

        private void ClearLastPosition(Folder folder, Entities.File file)
        {
            _logger.ClearLastPosition(file.RelativePath);
            foreach(var destination in folder.Destinations)
            {
                _transferStore.ClearLastPosition(destination, folder, file);
            }
        }

        private async Task SendToDestination(int sequenceNumber, byte[] buffer, Folder folder, Entities.File file, Destination destination)
        {
            var readBytes = _currentBytesRead.TryGetValue(destination.Name, out int currentBytesRead) ? currentBytesRead + buffer.Length : buffer.Length;
            try
            {
                _logger.SendSegment(file.RelativePath,destination.Name);
                await _transferService.Send(destination, new(sequenceNumber, buffer, file.Id, folder.Name));
            }
            catch (Exception ex)
            {
                _logger.SendSegmentFailed(file.RelativePath,destination.Name, ex);
                var start = readBytes > 0 ? readBytes - buffer.Length : 0;
                var end = readBytes;
                await _transferStore.StoreFailedSegmentTransfer(destination, folder, file, sequenceNumber, new SegmentRange(start, end));
            }
            finally
            {
                //even if transfer failed, you have read the bytes till there. Lastposition is only needed if service stops, and you want to continue from where you stopped
                await _transferStore.StoreLastPosition(destination, folder, file, readBytes);
                _currentBytesRead[destination.Name] = readBytes;

            }
        }

        private async Task SendReceipt(Folder folder, Entities.File file, int totalAmountOfSegments)
        {
            foreach (var destination in folder.Destinations)
            {
                try
                {
                    _logger.SendReceipt(file.RelativePath,destination.Name);
                    await _transferService.SendReceipt(destination, new(file.Id, file.RelativePath, totalAmountOfSegments, folder.Name));
                }
                catch (Exception ex)
                {
                    _logger.SendReceiptFailed(file.RelativePath,destination.Name, ex);
                    await _transferStore.StoreFailedReceiptTransfer(destination, folder, file, totalAmountOfSegments);
                }
            }
        }

    }

}
