using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public abstract class RetryTransferSegmentCommandHandlerBase
{
    public RetryTransferSegmentCommandHandlerBase(Segmentation segmentation, IFileTransferRepository fileTransferRepository, TransferService transferService, ILogger logger)
    {
        Segmentation = segmentation;
        FileTransferRepository = fileTransferRepository;
        TransferService = transferService;
        Logger = logger;
    }

    protected Segmentation Segmentation { get; }
    protected IFileTransferRepository FileTransferRepository { get; }
    protected TransferService TransferService { get; }
    protected ILogger Logger { get; }

    protected async Task Handle(FailedSegment failedSegment)
    {
        try
        {
            Logger.RetryTransferSegment(failedSegment.Sequencenumber, failedSegment.File.GetFullPath(), failedSegment.Destination.Name);
            var buffer = await Segmentation.GetSpecificSegment(failedSegment.File, failedSegment.Range);
            await TransferService.Send(failedSegment.Destination, new(failedSegment.Sequencenumber, buffer, failedSegment.File.Id, failedSegment.File.Folder.Name));
            FileTransferRepository.RemoveFailedSegment(failedSegment);
            await FileTransferRepository.UnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Logger.FailedRetryTransferSegment(failedSegment.Sequencenumber, failedSegment.File.GetFullPath(), failedSegment.Destination.Name, ex);
        }
    }
}
