using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class RetryTransferDeltaSegmentCommand : IRequest<Unit>
{
    public RetryTransferDeltaSegmentCommand(FailedSegment failedSegment)
    {
        FailedSegment = failedSegment;
    }

    public FailedSegment FailedSegment { get; }


    public class RetryTransferDeltaSegmentCommandHandler : RetryTransferSegmentCommandHandlerBase, IRequestHandler<RetryTransferDeltaSegmentCommand>
    {
        public RetryTransferDeltaSegmentCommandHandler(DeltaSegmentation deltaSegmentation, TransferService transferService
            , IFileTransferRepository fileTransferRepository, ILogger<RetryTransferFileSegmentCommand> logger)
            : base(deltaSegmentation,fileTransferRepository, transferService, logger)
        {
        }

        public async Task<Unit> Handle(RetryTransferDeltaSegmentCommand request, CancellationToken cancellationToken)
        {
            await Handle(request.FailedSegment);

            return Unit.Value;
        }
    }
}
