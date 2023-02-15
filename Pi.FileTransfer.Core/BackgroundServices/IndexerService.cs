using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Files.Commands;
using Pi.FileTransfer.Core.Folders.Commands;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Core.BackgroundServices;
public class IndexerService : BackgroundService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFolderRepository _folderRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<IndexerService> _logger;

    public IndexerService(IFileSystem fileSystem, IFolderRepository folderRepository, IMediator mediator, ILogger<IndexerService> logger)
    {
        _fileSystem = fileSystem;
        _folderRepository = folderRepository;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await IndexFolders();
            await IndexFiles();

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task IndexFolders()
    {
        _logger.IndexingFolders();
        var foldersWithoutSyncFolder = _fileSystem.GetFoldersFromBasePath()
            .Where(x => !Directory.Exists(Path.Combine(x, Constants.RootDirectoryName)));
        foreach (var folder in foldersWithoutSyncFolder)
        {
            await _mediator.Send(new IndexFolderCommand(folder));
        }
    }

    private async Task IndexFiles()
    {
        _logger.IndexingFiles();
        foreach (var folder in await _folderRepository.GetFolders())
        {
            await _mediator.Send(new IndexFilesCommand(folder));
            _folderRepository.Update(folder);
        }
        await _folderRepository.UnitOfWork.SaveChangesAsync();
    }
}
