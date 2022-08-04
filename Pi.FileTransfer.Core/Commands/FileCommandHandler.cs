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
public abstract class FileCommandHandlerBase
{
    protected ILogger Logger { get; }
    protected TransferService TransferService { get; }
    protected DataStore DataStore { get; }
    public bool IsFileUpdate { get; }

    //private readonly DataStore _transferStore;
    //private readonly DeltaService _deltaService;
    private readonly ConcurrentDictionary<string, int> _currentBytesRead = new();

    public FileCommandHandlerBase(ILogger logger, TransferService transferService, DataStore dataStore, bool isFileUpdate)
    {
        Logger = logger;
        TransferService = transferService;
        DataStore = dataStore;
        IsFileUpdate = isFileUpdate;
    }


    protected async Task SendSegment(int sequenceNumber, byte[] buffer, Entities.Folder folder, Entities.File file)
    {
        var runningTasks = new List<Task>();
        foreach (var destination in folder.Destinations)
        {
            runningTasks.Add(SendToDestination(sequenceNumber, buffer, folder, file, destination));
        }

        await Task.WhenAll(runningTasks);
        ClearLastPosition(folder, file);
    }

    protected void ClearLastPosition(Folder folder, Entities.File file)
    {
        Logger.ClearLastPosition(file.RelativePath);
        foreach (var destination in folder.Destinations)
        {
            DataStore.ClearLastPosition(destination, folder, file);
        }
    }

    protected async Task SendToDestination(int sequenceNumber, byte[] buffer, Folder folder, Entities.File file, Destination destination)
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
            await DataStore.StoreFailedSegmentTransfer(destination, folder, file, sequenceNumber, new SegmentRange(start, end),IsFileUpdate);
        }
        finally
        {
            //even if transfer failed, you have read the bytes till there. Lastposition is only needed if service stops, and you want to continue from where you stopped
            await DataStore.StoreLastPosition(destination, folder, file, readBytes);
            _currentBytesRead[destination.Name] = readBytes;

        }
    }
}
