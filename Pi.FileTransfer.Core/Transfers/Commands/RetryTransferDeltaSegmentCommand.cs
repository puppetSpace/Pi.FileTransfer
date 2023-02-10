using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class RetryTransferDeltaSegmentCommand : IRequest<Unit>
{
    public RetryTransferDeltaSegmentCommand(FailedSegment failedSegment, Destination destination, Folder folder)
    {
        FailedSegment = failedSegment;
        Destination = destination;
        Folder = folder;
    }

    public FailedSegment FailedSegment { get; }

    public Destination Destination { get; }

    public Folder Folder { get; }


    public class RetryTransferDeltaSegmentCommandHandler : RetryTransferSegmentCommandHandlerBase, IRequestHandler<RetryTransferDeltaSegmentCommand>
    {
        public RetryTransferDeltaSegmentCommandHandler(DeltaSegmentation deltaSegmentation, TransferService transferService
            , DataStore transferStore, ILogger<RetryTransferFileSegmentCommand> logger)
            : base(deltaSegmentation, transferService, transferStore, logger)
        {
        }

        public async Task<Unit> Handle(RetryTransferDeltaSegmentCommand request, CancellationToken cancellationToken)
        {
            await Handle(request.FailedSegment, request.Destination, request.Folder);

            return Unit.Value;
        }
    }
}
