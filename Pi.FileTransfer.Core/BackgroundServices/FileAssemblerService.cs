using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Files.Commands;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.BackgroundServices;
public class FileAssemblerService : BackgroundService
{
    private readonly IFolderRepository _folderRepository;
    private readonly DataStore _transferStore;
    private readonly IMediator _mediator;
    private readonly ILogger<FileAssemblerService> _logger;

    public FileAssemblerService(IFolderRepository folderRepository, DataStore transferStore, IMediator mediator, ILogger<FileAssemblerService> logger)
    {
        _folderRepository = folderRepository;
        _transferStore = transferStore;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var folder in _folderRepository.GetFolders())
            {
                _logger.SearchReceiptInFolder(folder.FullName);
                await foreach (var receipt in _transferStore.GetReceivedReceipts(folder))
                {
                    var files = (await _transferStore.GetReceivedSegmentsForFile(folder, receipt.FileId).ToListAsync(cancellationToken: stoppingToken));
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
