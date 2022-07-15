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
public class RetryTransferSegmentCommand : IRequest<MediatR.Unit>
{
    public RetryTransferSegmentCommand(FailedSegment failedSegment, Destination destination, Folder folder)
    {
        FailedSegment = failedSegment;
        Destination = destination;
        Folder = folder;
    }

    public FailedSegment FailedSegment { get; }

    public Destination Destination { get; }

    public Folder Folder { get; }


    public class RetryTransferSegmentCommandHandler : IRequestHandler<RetryTransferSegmentCommand>
    {
        private readonly FileSegmentation _fileSegmentation;
        private readonly TransferService _transferService;
        private readonly DataStore _transferStore;
        private readonly ILogger<RetryTransferSegmentCommand> _logger;

        public RetryTransferSegmentCommandHandler(FileSegmentation fileSegmentation, TransferService transferService, DataStore transferStore, ILogger<RetryTransferSegmentCommand> logger)
        {
            _fileSegmentation = fileSegmentation;
            _transferService = transferService;
            _transferStore = transferStore;
            _logger = logger;
        }

        public async Task<Unit> Handle(RetryTransferSegmentCommand request, CancellationToken cancellationToken)
        {
            var file = request.Folder.Files.SingleOrDefault(x => x.Id == request.FailedSegment.FileId);
            try
            {
                if (file is not null)
                {
                    _logger.RetryTransferSegment(request.FailedSegment.Sequencenumber,request.FailedSegment.FileId,request.Folder.Name,request.Destination.Name);
                    var buffer = await _fileSegmentation.GetSpecificSegment(request.Folder, file, request.FailedSegment.Range);
                    await _transferService.Send(request.Destination, new(request.FailedSegment.Sequencenumber, buffer, file.Id, request.Folder.Name));
                    _transferStore.DeleteFailedSegment(request.Destination, request.Folder, request.FailedSegment);
                }
                else
                {
                    _logger.DeleteFailedItems(request.FailedSegment.FileId,request.Folder.Name);
                    _transferStore.DeleteFailedItemsOfFile(request.Destination, request.Folder, request.FailedSegment.FileId);
                }
            }
            catch (Exception ex)
            {
                _logger.FailedRetryTransferSegment(request.FailedSegment.Sequencenumber,request.FailedSegment.FileId,request.Folder.Name,request.Destination.Name,ex);
            }

            return Unit.Value;
        }
    }
}
