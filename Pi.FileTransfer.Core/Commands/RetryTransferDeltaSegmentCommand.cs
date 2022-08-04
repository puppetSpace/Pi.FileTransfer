using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class RetryTransferDeltaSegmentCommand : IRequest<MediatR.Unit>
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


    public class RetryTransferDeltaSegmentCommandHandler : RetryTransferSegmentCommandHandler, IRequestHandler<RetryTransferDeltaSegmentCommand>
    {
        public RetryTransferDeltaSegmentCommandHandler(DeltaSegmentation deltaSegmentation, TransferService transferService
            , DataStore transferStore, ILogger<RetryTransferFileSegmentCommand> logger)
            : base(deltaSegmentation, transferService, transferStore, logger)
        {
        }

        public async Task<Unit> Handle(RetryTransferDeltaSegmentCommand request, CancellationToken cancellationToken)
        {
            await base.Handle(request.FailedSegment, request.Destination, request.Folder);

            return Unit.Value;
        }
    }
}
