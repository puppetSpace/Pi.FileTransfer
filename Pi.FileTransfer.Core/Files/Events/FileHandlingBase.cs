using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers;
using Pi.FileTransfer.Core.Transfers.Services;
using System.Collections.Concurrent;

namespace Pi.FileTransfer.Core.Files.Events;
public abstract class FileHandlingBase
{
    protected ILogger Logger { get; }
    protected TransferService TransferService { get; }
    protected IFileTransferRepository FileTransferRepository { get; }
    public bool IsFileUpdate { get; }

    protected ConcurrentDictionary<string, int> CurrentBytesRead { get; } = new();

    public FileHandlingBase(ILogger logger, TransferService transferService, IFileTransferRepository fileTransferRepository, bool isFileUpdate)
    {
        Logger = logger;
        TransferService = transferService;
        IsFileUpdate = isFileUpdate;
        FileTransferRepository = fileTransferRepository;
    }


    protected async Task SendSegment(int sequenceNumber, byte[] buffer, Folder folder, File file)
    {
        var runningTasks = new List<Task>();
        foreach (var destination in folder.Destinations)
        {
            runningTasks.Add(SendToDestination(sequenceNumber, buffer, folder, file, destination));
        }

        await Task.WhenAll(runningTasks);
        await FileTransferRepository.ClearLastPosition(file);
    }

    protected async Task SendToDestination(int sequenceNumber, byte[] buffer, Folder folder, File file, Destination destination)
    {
        var readBytes = CurrentBytesRead.TryGetValue(destination.Name, out var currentBytesRead) ? currentBytesRead + buffer.Length : buffer.Length;
        try
        {
            Logger.SendSegment(file.RelativePath, destination.Name);
            await TransferService.Send(destination, new(sequenceNumber, buffer, file.Id, folder.Name));
        }
        catch (Exception ex)
        {
            Logger.SendSegmentFailed(file.RelativePath, destination.Name, ex);
            var start = readBytes > 0 ? readBytes - buffer.Length : 0;
            var end = readBytes;
            FileTransferRepository.AddFailedSegment(new (file, destination, sequenceNumber, new SegmentRange(start, end), IsFileUpdate));
        }
        finally
        {
            //even if transfer failed, you have read the bytes till there. Lastposition is only needed if service stops, and you want to continue from where you stopped
            await FileTransferRepository.AddOrUpdateLastPosition(new(file,destination,readBytes));
            CurrentBytesRead[destination.Name] = readBytes;

        }
    }
}
