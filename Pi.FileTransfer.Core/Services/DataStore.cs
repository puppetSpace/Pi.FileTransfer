using MediatR;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Commands;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
public class DataStore
{
    private readonly IOptions<AppSettings> _options;
    private readonly IFileSystem _fileSystem;
    private readonly IMediator _mediator;

    public DataStore(IOptions<AppSettings> options, IFileSystem fileSystem, IMediator mediator)
    {
        _options = options;
        _fileSystem = fileSystem;
        _mediator = mediator;
    }

    public async Task StoreLastPosition(Destination destination, Entities.Folder folder, Entities.File file, int lastPosition)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
        _fileSystem.CreateDirectory(path);

        var transferFile = Path.Combine(path, file.Id.ToString());
        await _fileSystem.WritetoFile(transferFile, lastPosition);
    }

    public async Task StoreFailedSegmentTransfer(Destination destination, Entities.Folder folder, Entities.File file, int sequencenumber, SegmentRange range)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        _fileSystem.CreateDirectory(path);

        var transferFile = Path.Combine(path, $"{sequencenumber}_{file.Id}.failedsegment");
        await _fileSystem.WritetoFile(transferFile, new FailedSegment(file.Id,sequencenumber,range));
    }

    public async Task StoreFailedReceiptTransfer(Destination destination, Entities.Folder folder, Entities.File file, int totalAmountOfSegments)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        _fileSystem.CreateDirectory(path);

        var transferFile = Path.Combine(path, $"{file.Id}.failedreceipt");
        await _fileSystem.WritetoFile(transferFile, new FailedReceipt(file.Id, totalAmountOfSegments));
    }

    public async IAsyncEnumerable<FailedSegment> GetFailedSegments(Entities.Folder folder, Destination destination)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        if (Directory.Exists(path))
        {
            foreach (var file in _fileSystem.GetFiles(path, "*.failedsegment"))
                yield return await _fileSystem.GetContentOfFile<FailedSegment>(file);
        }

    }

    public async IAsyncEnumerable<FailedReceipt> GetFailedReceipts(Entities.Folder folder, Destination destination)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        if (Directory.Exists(path))
        {
            foreach (var file in _fileSystem.GetFiles(path, "*.failedreceipt"))
                yield return await _fileSystem.GetContentOfFile<FailedReceipt>(file);
        }

    }

    public async Task StoreReceivedSegment(TransferSegment transferSegment)
    {
        var folder = Path.Combine(_options.Value.BasePath, transferSegment.FolderName);
        await _mediator.Send(new IndexFolderCommand(folder));

        var incomingDataFolder = Path.Combine(folder, Constants.RootDirectoryName, "Data", "Incoming", transferSegment.FileId.ToString());
        _fileSystem.CreateDirectory(incomingDataFolder);

        var segmentPath = Path.Combine(incomingDataFolder, $"{transferSegment.Sequencenumber}_{transferSegment.FileId}.segment");

        await _fileSystem.WritetoFile(segmentPath, transferSegment); ;


    }

    public async IAsyncEnumerable<TransferSegment> GetReceivedSegmentsForFile(Folder folder, Guid fileId)
    {
        var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming", fileId.ToString());
        if (Directory.Exists(incomingDataFolder))
        {
            foreach (var file in _fileSystem.GetFiles(incomingDataFolder, "*.segment"))
                yield return await _fileSystem.GetContentOfFile<TransferSegment>(file);
        }
    }

    public async Task StoreReceivedReceipt(TransferReceipt transferReceipt)
    {
        var folder = Path.Combine(_options.Value.BasePath, transferReceipt.FolderName);
        await _mediator.Send(new IndexFolderCommand(folder));

        var incomingDataFolder = Path.Combine(folder, Constants.RootDirectoryName, "Data", "Incoming", transferReceipt.FileId.ToString());
        _fileSystem.CreateDirectory(incomingDataFolder);

        var receiptPath = Path.Combine(incomingDataFolder, $"{transferReceipt.FileId}.receipt");

        await _fileSystem.WritetoFile(receiptPath, transferReceipt); ;


    }
    public async IAsyncEnumerable<TransferReceipt> GetReceivedReceipts(Folder folder)
    {
        var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming");
        if (Directory.Exists(incomingDataFolder))
        {
            foreach (var file in _fileSystem.GetFiles(incomingDataFolder, "*.receipt",true))
                yield return await _fileSystem.GetContentOfFile<TransferReceipt>(file);
        }
    }
    internal void DeleteFailedItemsOfFile(Destination destination, Folder folder, Entities.File? file)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, $"*{file.Id}*");
                foreach (var toDelete in files)
                {
                    _fileSystem.DeleteFile(toDelete);
                }
            }
        }
        catch (Exception ex)
        {
            //todo log
        }
    }
    internal void DeleteFailedSegment(Destination destination, Folder folder, FailedSegment failedSegment)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var segmentFile = Path.Combine(path, $"{failedSegment.Sequencenumber}_{failedSegment.FileId}.failedsegment");
                _fileSystem.DeleteFile(segmentFile);
            }
        }
        catch (Exception ex)
        {

            //todo log
        }
    }

    internal void DeleteFailedReceipt(Destination destination, Folder folder, FailedReceipt failedReceipt)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var receiptFile = Path.Combine(path, $"{failedReceipt.FileId}.failedreceipt");
                _fileSystem.DeleteFile(receiptFile);
            }
        }
        catch (Exception ex)
        {

            //todo log
        }
    }

    internal void DeleteReceivedDataOfFile(Folder folder, Guid fileId)
    {
        var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming", fileId.ToString());
        _fileSystem.DeleteDirectory(incomingDataFolder);
    }

    internal void ClearLastPosition(Destination destination, Folder folder, Entities.File file)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
        var transferFile = Path.Combine(path, file.Id.ToString());
         _fileSystem.DeleteFile(transferFile);
    }
}
