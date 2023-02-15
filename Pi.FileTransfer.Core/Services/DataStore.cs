using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Folders.Commands;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Receives;

namespace Pi.FileTransfer.Core.Services;
internal class DataStore
{
    private readonly IOptions<AppSettings> _options;
    private readonly FolderState _folderState;
    private readonly IFileSystem _fileSystem;
    private readonly IMediator _mediator;
    private readonly ILogger<DataStore> _logger;

    public DataStore(IOptions<AppSettings> options, FolderState folderState, IFileSystem fileSystem, IMediator mediator, ILogger<DataStore> logger)
    {
        _options = options;
        _folderState = folderState;
        _fileSystem = fileSystem;
        _mediator = mediator;
        _logger = logger;
    }


    public async Task StoreReceivedSegment(Segment transferSegment)
    {
        try
        {
            _logger.StoreReceivedSegment(transferSegment.Sequencenumber, transferSegment.FileId, transferSegment.FolderName);

            var folder = Path.Combine(_options.Value.BasePath, transferSegment.FolderName);
            if(!_folderState.Exists(folder))
                await _mediator.Send(new IndexFolderCommand(folder));

            var incomingDataFolder = FolderUtils.GetIncomingFolderPath(folder, transferSegment.FileId.ToString());
            _fileSystem.CreateDirectory(incomingDataFolder);

            var segmentPath = Path.Combine(incomingDataFolder, $"{transferSegment.Sequencenumber}_{transferSegment.FileId}.segment");

            await _fileSystem.WritetoFile(segmentPath, transferSegment);
        }
        catch (Exception ex)
        {
            _logger.FailedToStoreReceivedSegment(transferSegment.Sequencenumber, transferSegment.FileId, transferSegment.FolderName, ex);
        }
    }

    public async IAsyncEnumerable<Segment> GetReceivedSegmentsForFile(Folder folder, Guid fileId)
    {
        var incomingDataFolder = FolderUtils.GetIncomingFolderPath(folder.FullName, fileId.ToString());
        if (Directory.Exists(incomingDataFolder))
        {
            var segmentFiles = _fileSystem.GetFiles(incomingDataFolder, "*.segment").ToList();
            _logger.GetReceivedSegments(segmentFiles.Count, fileId, folder.Name);
            foreach (var file in segmentFiles)
            {
                Segment? content = null;
                try
                {
                    content = await _fileSystem.GetContentOfFile<Segment>(file);
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

    public void DeleteFailedItemsOfFile(Destination destination, Folder folder, Guid fileId)
    {
        //todo do in database
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

    public void DeleteReceivedDataOfFile(Folder folder, Guid fileId)
    {
        try
        {
            _logger.DeleteReceivedData(folder.Name, fileId);
            var incomingDataFolder = FolderUtils.GetIncomingFolderPath(folder.FullName, fileId.ToString());
            _fileSystem.DeleteDirectory(incomingDataFolder);
        }
        catch (Exception ex)
        {
            _logger.FailedToDeleteReceivedData(folder.Name, fileId, ex);
        }
    }
}
