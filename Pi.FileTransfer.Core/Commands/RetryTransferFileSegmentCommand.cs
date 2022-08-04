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
public class RetryTransferFileSegmentCommand : IRequest<MediatR.Unit>
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


    public class RetryTransferFileSegmentCommandHandler : RetryTransferSegmentCommandHandler, IRequestHandler<RetryTransferFileSegmentCommand>
    {

        public RetryTransferFileSegmentCommandHandler(FileSegmentation fileSegmentation, TransferService transferService
            , DataStore transferStore, ILogger<RetryTransferFileSegmentCommand> logger)
            :base(fileSegmentation,transferService,transferStore,logger)
        {
        }

        public async Task<Unit> Handle(RetryTransferFileSegmentCommand request, CancellationToken cancellationToken)
        {
            await base.Handle(request.FailedSegment, request.Destination, request.Folder);

            return Unit.Value;
        }
    }
}
