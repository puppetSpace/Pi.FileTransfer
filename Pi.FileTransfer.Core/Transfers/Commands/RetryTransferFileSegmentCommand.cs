using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class RetryTransferFileSegmentCommand : IRequest<Unit>
{
    public RetryTransferFileSegmentCommand(FailedSegment failedSegment, Destination destination, Folder folder)
    {
        FailedSegment = failedSegment;
        Destination = destination;
        Folder = folder;
    }

    public FailedSegment FailedSegment { get; }

    public Destination Destination { get; }

    public Folder Folder { get; }


    public class RetryTransferFileSegmentCommandHandler : RetryTransferSegmentCommandHandlerBase, IRequestHandler<RetryTransferFileSegmentCommand>
    {

        public RetryTransferFileSegmentCommandHandler(FileSegmentation fileSegmentation, TransferService transferService
            , DataStore transferStore, ILogger<RetryTransferFileSegmentCommand> logger)
            : base(fileSegmentation, transferService, transferStore, logger)
        {
        }

        public async Task<Unit> Handle(RetryTransferFileSegmentCommand request, CancellationToken cancellationToken)
        {
            await Handle(request.FailedSegment, request.Destination, request.Folder);

            return Unit.Value;
        }
    }
}
