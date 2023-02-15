using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class RetryTransferFileSegmentCommand : IRequest<Unit>
{
    public RetryTransferFileSegmentCommand(FailedSegment failedSegment)
    {
        FailedSegment = failedSegment;
    }

    public FailedSegment FailedSegment { get; }

    public class RetryTransferFileSegmentCommandHandler : RetryTransferSegmentCommandHandlerBase, IRequestHandler<RetryTransferFileSegmentCommand>
    {

        public RetryTransferFileSegmentCommandHandler(FileSegmentation fileSegmentation, TransferService transferService
            , IFileTransferRepository fileTransferRepository, ILogger<RetryTransferFileSegmentCommand> logger)
            : base(fileSegmentation,fileTransferRepository, transferService, logger)
        {
        }

        public async Task<Unit> Handle(RetryTransferFileSegmentCommand request, CancellationToken cancellationToken)
        {
            await Handle(request.FailedSegment);

            return Unit.Value;
        }
    }
}
