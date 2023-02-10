using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public abstract class RetryTransferSegmentCommandHandlerBase
{
    public RetryTransferSegmentCommandHandlerBase(Segmentation segmentation, TransferService transferService, DataStore dataStore, ILogger logger)
    {
        Segmentation = segmentation;
        TransferService = transferService;
        DataStore = dataStore;
        Logger = logger;
    }

    protected Segmentation Segmentation { get; }
    protected TransferService TransferService { get; }
    protected DataStore DataStore { get; }
    protected ILogger Logger { get; }

    protected async Task Handle(FailedSegment failedSegment, Destination destination, Folder folder)
    {
        var file = folder.Files.SingleOrDefault(x => x.Id == failedSegment.FileId);
        try
        {
            if (file is not null)
            {
                Logger.RetryTransferSegment(failedSegment.Sequencenumber, failedSegment.FileId, folder.Name, destination.Name);
                var buffer = await Segmentation.GetSpecificSegment(folder, file, failedSegment.Range);
                await TransferService.Send(destination, new(failedSegment.Sequencenumber, buffer, file.Id, folder.Name));
                DataStore.DeleteFailedSegment(destination, folder, failedSegment);
            }
            else
            {
                Logger.DeleteFailedItems(failedSegment.FileId, folder.Name);
                DataStore.DeleteFailedItemsOfFile(destination, folder, failedSegment.FileId);
            }
        }
        catch (Exception ex)
        {
            Logger.FailedRetryTransferSegment(failedSegment.Sequencenumber, failedSegment.FileId, folder.Name, destination.Name, ex);
        }
    }
}
