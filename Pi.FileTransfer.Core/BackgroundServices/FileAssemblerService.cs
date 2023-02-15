using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Files.Commands;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.BackgroundServices;
internal class FileAssemblerService : BackgroundService
{
    private readonly IFolderRepository _folderRepository;
    private readonly IFileReceiveRepository _fileReceiveRepository;
    private readonly DataStore _dataStore;
    private readonly IMediator _mediator;
    private readonly ILogger<FileAssemblerService> _logger;

    public FileAssemblerService(IFolderRepository folderRepository,IFileReceiveRepository fileReceiveRepository, DataStore dataStore, IMediator mediator, ILogger<FileAssemblerService> logger)
    {
        _folderRepository = folderRepository;
        _fileReceiveRepository = fileReceiveRepository;
        _dataStore = dataStore;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var folder in await _folderRepository.GetFolders())
            {
                _logger.SearchReceiptInFolder(folder.FullName);
                foreach (var receipt in await _fileReceiveRepository.GetReceipts(folder.Name))
                {
                    var files = (await _dataStore.GetReceivedSegmentsForFile(folder, receipt.FileId).ToListAsync(cancellationToken: stoppingToken));
                    _logger.AmountOfSegmentFilesPresent(receipt.RelativePath, receipt.AmountOfSegments, files.Count);
                    if (receipt.AmountOfSegments == files.Count)
                    {
                        IRequest<Unit> request;
                        if (receipt.IsFileUpdate)
                            request = new ApplyDeltaCommand(folder, receipt, files!);
                        else
                            request = new AssembleFileCommand(folder, receipt, files!);

                        _ = await _mediator.Send(request, stoppingToken);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
