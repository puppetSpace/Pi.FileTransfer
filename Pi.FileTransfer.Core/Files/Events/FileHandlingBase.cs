using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers;
using Pi.FileTransfer.Core.Transfers.Services;
using System.Collections.Concurrent;

namespace Pi.FileTransfer.Core.Files.Events;
public abstract class FileHandlingBase
{
    protected ILogger Logger { get; }
    protected TransferService TransferService { get; }
    protected DataStore DataStore { get; }
    public bool IsFileUpdate { get; }

    private readonly ConcurrentDictionary<string, int> _currentBytesRead = new();

    public FileHandlingBase(ILogger logger, TransferService transferService, DataStore dataStore, bool isFileUpdate)
    {
        Logger = logger;
        TransferService = transferService;
        DataStore = dataStore;
        IsFileUpdate = isFileUpdate;
    }


    protected async Task SendSegment(int sequenceNumber, byte[] buffer, Folder folder, File file)
    {
        var runningTasks = new List<Task>();
        foreach (var destination in folder.Destinations)
        {
            runningTasks.Add(SendToDestination(sequenceNumber, buffer, folder, file, destination));
        }

        await Task.WhenAll(runningTasks);
        ClearLastPosition(folder, file);
    }

    protected void ClearLastPosition(Folder folder, File file)
    {
        Logger.ClearLastPosition(file.RelativePath);
        foreach (var destination in folder.Destinations)
        {
            DataStore.ClearLastPosition(destination, folder, file);
        }
    }

    protected async Task SendToDestination(int sequenceNumber, byte[] buffer, Folder folder, File file, Destination destination)
    {
        var readBytes = _currentBytesRead.TryGetValue(destination.Name, out var currentBytesRead) ? currentBytesRead + buffer.Length : buffer.Length;
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
            await DataStore.StoreFailedSegmentTransfer(destination, folder, file, sequenceNumber, new SegmentRange(start, end), IsFileUpdate);
        }
        finally
        {
            //even if transfer failed, you have read the bytes till there. Lastposition is only needed if service stops, and you want to continue from where you stopped
            await DataStore.StoreLastPosition(destination, folder, file, readBytes);
            _currentBytesRead[destination.Name] = readBytes;

        }
    }
}
