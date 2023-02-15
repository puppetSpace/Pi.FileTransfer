using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Transfers.Commands;

namespace Pi.FileTransfer.Core.BackgroundServices;
public class RetryService : BackgroundService
{
    private readonly IFolderRepository _folderRepository;
    private readonly IFileTransferRepository _fileTransferRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<RetryService> _logger;

    public RetryService(IFolderRepository folderRepository, IFileTransferRepository fileTransferRepository, IMediator mediator, ILogger<RetryService> logger)
    {
        _folderRepository = folderRepository;
        _fileTransferRepository = fileTransferRepository;
        _mediator = mediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var folder in await _folderRepository.GetFolders())
            {
                foreach (var destination in folder.Destinations)
                {
                    await RetrySendingSegments(folder, destination);
                    await RetrySendingReceipt(folder, destination);
                    await RetrySendingFileFromLastPosition(folder, destination);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task RetrySendingSegments(Folder folder, Destination destination)
    {
        _logger.RetrySendingSegments(folder.Name, destination.Name);
        foreach (var failure in await _fileTransferRepository.GetFailedSegments(folder, destination))
        {
            IRequest<Unit> request;
            if (failure.IsFileUpdate)
                request = new RetryTransferDeltaSegmentCommand(failure);
            else
                request = new RetryTransferFileSegmentCommand(failure);

            _ = await _mediator.Send(request);
        }
    }

    private async Task RetrySendingReceipt(Folder folder, Destination destination)
    {
        _logger.RetrySendingReceipts(folder.Name, destination.Name);
        foreach (var failure in await _fileTransferRepository.GetFailedReceipts(folder, destination))
        {
            _ = await _mediator.Send(new RetryTransferReceiptCommand(failure));
        }
    }

    private async Task RetrySendingFileFromLastPosition(Folder folder, Destination destination)
    {
        foreach (var file in folder.Files)
        {
            var lastPosition = await _fileTransferRepository.GetLastPosition(file, destination);
            if (lastPosition is not null)
            {
                _logger.RetrySendingFileFromLastPosition(file.Name, folder.Name, destination.Name);
                _ = await _mediator.Send(new RestartTransferFileCommand(file, folder, destination, lastPosition.ReadBytes));
            }
        }
    }
}
