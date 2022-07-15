using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<DataStore> _logger;

    public DataStore(IOptions<AppSettings> options, IFileSystem fileSystem, IMediator mediator, ILogger<DataStore> logger)
    {
        _options = options;
        _fileSystem = fileSystem;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task StoreLastPosition(Destination destination, Entities.Folder folder, Entities.File file, int lastPosition)
    {
        _logger.LogDebug($"Storing last positition '{lastPosition}' of file '{file.RelativePath}' for destination '{destination.Name}'");
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
        _fileSystem.CreateDirectory(path);

        var transferFile = Path.Combine(path, file.Id.ToString());
        await _fileSystem.WritetoFile(transferFile, lastPosition);
    }

    public async Task StoreFailedSegmentTransfer(Destination destination, Entities.Folder folder, Entities.File file, int sequencenumber, SegmentRange range)
    {
        _logger.LogDebug($"Storing failedsegment '{sequencenumber}' of file '{file.RelativePath}' for destination '{destination.Name}'");
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        _fileSystem.CreateDirectory(path);

        var transferFile = Path.Combine(path, $"{sequencenumber}_{file.Id}.failedsegment");
        await _fileSystem.WritetoFile(transferFile, new FailedSegment(file.Id, sequencenumber, range));
    }

    public async Task StoreFailedReceiptTransfer(Destination destination, Entities.Folder folder, Entities.File file, int totalAmountOfSegments)
    {
        _logger.LogDebug($"Storing failed receipt of file '{file.RelativePath}' for destination '{destination.Name}'");
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
            var failedSegments = _fileSystem.GetFiles(path, "*.failedsegment").ToList();
            _logger.LogDebug($"Getting {failedSegments.Count} failed segments from folder {folder.Name}' for destination '{destination.Name}'");
            foreach (var file in failedSegments)
                yield return await _fileSystem.GetContentOfFile<FailedSegment>(file);
        }

    }

    public async IAsyncEnumerable<FailedReceipt> GetFailedReceipts(Entities.Folder folder, Destination destination)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        if (Directory.Exists(path))
        {
            var failedReceipts = _fileSystem.GetFiles(path, "*.failedreceipt").ToList();
            _logger.LogDebug($"Getting {failedReceipts.Count} failed receipts from folder {folder.Name}' for destination '{destination.Name}'");
            foreach (var file in failedReceipts)
                yield return await _fileSystem.GetContentOfFile<FailedReceipt>(file);
        }

    }

    public async Task StoreReceivedSegment(TransferSegment transferSegment)
    {
        _logger.LogDebug($"Storing received segment {transferSegment.Sequencenumber} of file with id '{transferSegment.FileId}' for folder '{transferSegment.FolderName}'");
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
            var segmentFiles = _fileSystem.GetFiles(incomingDataFolder, "*.segment").ToList();
            _logger.LogDebug($"Getting {segmentFiles.Count} received segments for file with id '{fileId}' of folder '{folder.Name}'");
            foreach (var file in segmentFiles)
                yield return await _fileSystem.GetContentOfFile<TransferSegment>(file);
        }
    }

    public async Task StoreReceivedReceipt(TransferReceipt transferReceipt)
    {
        _logger.LogDebug($"Storing received receipt of file '{transferReceipt.RelativePath}' for folder '{transferReceipt.FolderName}'");
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
            var receiptFiles = _fileSystem.GetFiles(incomingDataFolder, "*.receipt", true).ToList();
            _logger.LogDebug($"Getting {receiptFiles.Count} received receipts for folder '{folder.Name}'");
            foreach (var file in receiptFiles)
                yield return await _fileSystem.GetContentOfFile<TransferReceipt>(file);
        }
    }
    internal void DeleteFailedItemsOfFile(Destination destination, Folder folder, Guid fileId)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, $"*{fileId}*");
                foreach (var toDelete in files)
                {
                    _logger.LogDebug($"Deleting failed item of file with id '{fileId}' from folder '{folder.Name}' for destination '{destination.Name}' ");
                    _fileSystem.DeleteFile(toDelete);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception happend while deleting failed items of file with id '{fileId}' from folder '{folder.Name}' for destination '{destination.Name}'");
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
                _logger.LogDebug($"Deleting failed segment '{segmentFile}' for destination '{destination.Name}' ");
                _fileSystem.DeleteFile(segmentFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception happend while deleting failed segment '{failedSegment.Sequencenumber}' of file with id '{failedSegment.FileId}' from folder '{folder.Name}' for destination '{destination.Name}'");
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
                _logger.LogDebug($"Deleting failed receipt '{receiptFile}' for destination '{destination.Name}' ");
                _fileSystem.DeleteFile(receiptFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception happend while deleting failed receipt of file with id '{failedReceipt.FileId}' from folder '{folder.Name}' for destination '{destination.Name}'");
        }
    }

    internal void DeleteReceivedDataOfFile(Folder folder, Guid fileId)
    {
        _logger.LogDebug($"Deleting received data from folder {folder.Name} for file with id '{fileId}'");
        var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming", fileId.ToString());
        _fileSystem.DeleteDirectory(incomingDataFolder);
    }

    internal void ClearLastPosition(Destination destination, Folder folder, Entities.File file)
    {
        _logger.LogDebug($"Clear last position of file '{file.RelativePath}' for destination '{destination.Name}'");
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
        var transferFile = Path.Combine(path, file.Id.ToString());
        _fileSystem.DeleteFile(transferFile);
    }
}
