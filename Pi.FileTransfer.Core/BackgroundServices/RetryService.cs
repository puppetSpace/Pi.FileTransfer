using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Commands;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.BackgroundServices;
public class RetryService : BackgroundService
{
    private readonly IFolderRepository _folderRepository;
    private readonly DataStore _transferStore;
    private readonly IMediator _mediator;
    private readonly ILogger<RetryService> _logger;

    public RetryService(IFolderRepository folderRepository, DataStore transferStore, IMediator mediator, ILogger<RetryService> logger)
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
                foreach (var destination in folder.Destinations)
                {
                    await RetrySendingSegments(folder, destination);
                    await RetrySendingReceipt(folder, destination);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task RetrySendingSegments(Folder folder, Destination destination)
    {
        _logger.RetrySendingSegments(folder.Name,destination.Name);
        await foreach (var failure in _transferStore.GetFailedSegments(folder, destination))
        {
            _ = await _mediator.Send(new RetryTransferSegmentCommand(failure, destination, folder));
        }
    }

    private async Task RetrySendingReceipt(Folder folder, Destination destination)
    {
        _logger.RetrySendingReceipts(folder.Name,destination.Name);
        await foreach (var failure in _transferStore.GetFailedReceipts(folder, destination))
        {
            _ = await _mediator.Send(new RetryTransferReceiptCommand(failure, destination, folder));
        }
    }
}
