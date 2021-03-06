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
        try
        {
            _logger.StoreLastPosition(lastPosition, file.RelativePath, destination.Name);
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
            _fileSystem.CreateDirectory(path);

            var transferFile = Path.Combine(path, file.Id.ToString());
            await _fileSystem.WritetoFile(transferFile, lastPosition);
        }
        catch (Exception ex)
        {
            _logger.FailedToStoreLastPosition(lastPosition, file.RelativePath, destination.Name, ex);
        }
    }

    public async Task<int?> GetLastPosition(Entities.Folder folder, Destination destination, Guid fileId)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
            _fileSystem.CreateDirectory(path);

            var transferFile = Path.Combine(path, fileId.ToString());
            if (!System.IO.File.Exists(transferFile))
                return null;

            return await _fileSystem.GetContentOfFile<int>(transferFile);
        }
        catch
        {
            return null;
        }
    }

    public async Task StoreFailedSegmentTransfer(Destination destination, Entities.Folder folder, Entities.File file, int sequencenumber, SegmentRange range)
    {
        try
        {
            _logger.StoreFailedSegment(sequencenumber, file.RelativePath, destination.Name);
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            _fileSystem.CreateDirectory(path);

            var transferFile = Path.Combine(path, $"{sequencenumber}_{file.Id}.failedsegment");
            await _fileSystem.WritetoFile(transferFile, new FailedSegment(file.Id, sequencenumber, range));
        }
        catch (Exception ex)
        {
            _logger.FailedToStoreFailedSegment(sequencenumber, file.RelativePath, destination.Name, ex);
        }
    }

    public async Task StoreFailedReceiptTransfer(Destination destination, Entities.Folder folder, Entities.File file, int totalAmountOfSegments, bool isFileUpdate)
    {
        try
        {
            _logger.StoreFailedReceipt(file.RelativePath, destination.Name);
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            _fileSystem.CreateDirectory(path);

            var transferFile = Path.Combine(path, $"{file.Id}.failedreceipt");
            await _fileSystem.WritetoFile(transferFile, new FailedReceipt(file.Id, totalAmountOfSegments,isFileUpdate));
        }
        catch (Exception ex)
        {
            _logger.FailedToStoreFailedReceipt(file.RelativePath, destination.Name, ex);
        }
    }

    public async IAsyncEnumerable<FailedSegment> GetFailedSegments(Entities.Folder folder, Destination destination)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        if (Directory.Exists(path))
        {
            var failedSegments = _fileSystem.GetFiles(path, "*.failedsegment").ToList();
            _logger.GetFailedSegments(failedSegments.Count, folder.Name, destination.Name);
            foreach (var file in failedSegments)
            {
                FailedSegment? content = null;
                try
                {
                    content = await _fileSystem.GetContentOfFile<FailedSegment>(file);
                }
                catch (Exception ex)
                {
                    _logger.FailedToGetContentOfFailedSegment(file, ex);
                }
                if(content is not null)
                    yield return content;
            }
        }

    }

    public async IAsyncEnumerable<FailedReceipt> GetFailedReceipts(Entities.Folder folder, Destination destination)
    {
        var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
        if (Directory.Exists(path))
        {
            var failedReceipts = _fileSystem.GetFiles(path, "*.failedreceipt").ToList();
            _logger.GetFailedReceipts(failedReceipts.Count, folder.Name, destination.Name);
            foreach (var file in failedReceipts)
            {
                FailedReceipt? content = null;
                try
                {
                    content = await _fileSystem.GetContentOfFile<FailedReceipt>(file);
                }
                catch (Exception ex)
                {
                    _logger.FailedToGetContentOfFailedReceipt(file, ex);
                }
                if (content is not null)
                    yield return content;
            }
        }

    }

    public async Task StoreReceivedSegment(TransferSegment transferSegment)
    {
        try
        {
            _logger.StoreReceivedSegment(transferSegment.Sequencenumber, transferSegment.FileId, transferSegment.FolderName);
            var folder = Path.Combine(_options.Value.BasePath, transferSegment.FolderName);
            await _mediator.Send(new IndexFolderCommand(folder));

            var incomingDataFolder = Path.Combine(folder, Constants.RootDirectoryName, "Data", "Incoming", transferSegment.FileId.ToString());
            _fileSystem.CreateDirectory(incomingDataFolder);

            var segmentPath = Path.Combine(incomingDataFolder, $"{transferSegment.Sequencenumber}_{transferSegment.FileId}.segment");

            await _fileSystem.WritetoFile(segmentPath, transferSegment);
        }
        catch (Exception ex)
        {
            _logger.FailedToStoreReceivedSegment(transferSegment.Sequencenumber, transferSegment.FileId, transferSegment.FolderName, ex);
        }
    }

    public async Task StoreReceivedReceipt(TransferReceipt transferReceipt)
    {
        try
        {
            _logger.StoreReceivedReceipt(transferReceipt.RelativePath, transferReceipt.FolderName);
            var folder = Path.Combine(_options.Value.BasePath, transferReceipt.FolderName);
            await _mediator.Send(new IndexFolderCommand(folder));

            var incomingDataFolder = Path.Combine(folder, Constants.RootDirectoryName, "Data", "Incoming", transferReceipt.FileId.ToString());
            _fileSystem.CreateDirectory(incomingDataFolder);

            var receiptPath = Path.Combine(incomingDataFolder, $"{transferReceipt.FileId}.receipt");

            await _fileSystem.WritetoFile(receiptPath, transferReceipt); ;
        }
        catch (Exception ex)
        {
            _logger.FailedToStoreReceivedReceipt(transferReceipt.RelativePath, transferReceipt.FolderName, ex);
        }
    }

    public async IAsyncEnumerable<TransferSegment> GetReceivedSegmentsForFile(Folder folder, Guid fileId)
    {
        var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming", fileId.ToString());
        if (Directory.Exists(incomingDataFolder))
        {
            var segmentFiles = _fileSystem.GetFiles(incomingDataFolder, "*.segment").ToList();
            _logger.GetReceivedSegments(segmentFiles.Count, fileId, folder.Name);
            foreach (var file in segmentFiles)
            {
                TransferSegment? content = null;
                try
                {
                    content = await _fileSystem.GetContentOfFile<TransferSegment>(file);
                }
                catch (Exception ex)
                {
                    _logger.FailedToGetContentOfSegment(file, ex);
                }

                if (content is not null)
                    yield return content!;
            }
        }
    }

    public async IAsyncEnumerable<TransferReceipt> GetReceivedReceipts(Folder folder)
    {
        var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming");
        if (Directory.Exists(incomingDataFolder))
        {
            var receiptFiles = _fileSystem.GetFiles(incomingDataFolder, "*.receipt", true).ToList();
            _logger.GetReceivedReceipts(receiptFiles.Count, folder.Name);
            foreach (var file in receiptFiles)
            {
                TransferReceipt? content = null;
                try
                {
                    content = await _fileSystem.GetContentOfFile<TransferReceipt>(file);
                }
                catch (Exception ex)
                {
                    _logger.FailedToGetContentOfReceipt(file, ex);
                }
                if (content is not null)
                    yield return content!;
            }
        }
    }

    public void DeleteFailedItemsOfFile(Destination destination, Folder folder, Guid fileId)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, $"*{fileId}*");
                foreach (var toDelete in files)
                {
                    _logger.DeleteFailedItemOfFile(fileId, folder.Name, destination.Name);
                    _fileSystem.DeleteFile(toDelete);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.FailedToDeleteFailedItemOfFile(fileId, folder.Name, destination.Name, ex);
        }
    }

    public void DeleteFailedSegment(Destination destination, Folder folder, FailedSegment failedSegment)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var segmentFile = Path.Combine(path, $"{failedSegment.Sequencenumber}_{failedSegment.FileId}.failedsegment");
                _logger.DeleteFailedSegment(segmentFile, destination.Name);
                _fileSystem.DeleteFile(segmentFile);
            }
        }
        catch (Exception ex)
        {
            _logger.FailedToDeleteFailedSegment(failedSegment.Sequencenumber, failedSegment.FileId, folder.Name, destination.Name, ex);
        }
    }

    public void DeleteFailedReceipt(Destination destination, Folder folder, FailedReceipt failedReceipt)
    {
        try
        {
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name, "Failed");
            if (Directory.Exists(path))
            {
                var receiptFile = Path.Combine(path, $"{failedReceipt.FileId}.failedreceipt");
                _logger.DeleteFailedReceipt(receiptFile, destination.Name);
                _fileSystem.DeleteFile(receiptFile);
            }
        }
        catch (Exception ex)
        {
            _logger.FailedToDeleteFailedReceipt(failedReceipt.FileId, folder.Name, destination.Name, ex);
        }
    }

    public void DeleteReceivedDataOfFile(Folder folder, Guid fileId)
    {
        try
        {
            _logger.DeleteReceivedData(folder.Name, fileId);
            var incomingDataFolder = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Incoming", fileId.ToString());
            _fileSystem.DeleteDirectory(incomingDataFolder);
        }
        catch (Exception ex)
        {
            _logger.FailedToDeleteReceivedData(folder.Name, fileId, ex);
        }
    }

    public void ClearLastPosition(Destination destination, Folder folder, Entities.File file)
    {
        try
        {
            _logger.ClearLastPosition(file.RelativePath, destination.Name);
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Outgoing", destination.Name);
            var transferFile = Path.Combine(path, file.Id.ToString());
            _fileSystem.DeleteFile(transferFile);
        }
        catch (Exception ex)
        {
            _logger.FailedToClearLastPosition(file.RelativePath, destination.Name, ex);
        }
    }

    public Stream CreateSignatureFile(Folder folder, Entities.File file)
    {
        try
        {
            _logger.CreateSignatureFile(file.RelativePath,file.Id);
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Signatures");
            _fileSystem.CreateDirectory(path);
            var signatureFile = Path.Combine(path, file.Id.ToString());
            return _fileSystem.GetWriteFileStream(signatureFile);
        }
        catch (Exception ex)
        {
            _logger?.FailedToCreateSignatureFile(file.RelativePath, file.Id, ex);
            return FileStream.Null;
        }
    }

    public async Task<byte[]> GetSignatureFileContent(Folder folder, Entities.File file)
    {
        try
        {
            _logger.GetSignatureFileContent(file.RelativePath, file.Id);
            var path = Path.Combine(folder.FullName, Constants.RootDirectoryName, "Data", "Signatures", file.Id.ToString());
            if (System.IO.File.Exists(path))
            {
                return await _fileSystem.GetRawContentOfFile(path);
            }
            else
            {
                return Array.Empty<byte>();
            }
        }
        catch (Exception ex)
        {
            _logger?.FailedToGetGetSignatureFileContent(file.RelativePath, file.Id, ex);
            return Array.Empty<byte>();
        }
    }
}
