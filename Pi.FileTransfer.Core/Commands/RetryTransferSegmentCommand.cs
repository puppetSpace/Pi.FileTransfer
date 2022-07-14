using MediatR;
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

        public RetryTransferSegmentCommandHandler(FileSegmentation fileSegmentation, TransferService transferService, DataStore transferStore)
        {
            _fileSegmentation = fileSegmentation;
            _transferService = transferService;
            _transferStore = transferStore;
        }

        public async Task<Unit> Handle(RetryTransferSegmentCommand request, CancellationToken cancellationToken)
        {
            var file = request.Folder.Files.SingleOrDefault(x => x.Id == request.FailedSegment.FileId);
            try
            {
                if (file is not null)
                {
                    var buffer = await _fileSegmentation.GetSpecificSegment(request.Folder, file, request.FailedSegment.Range);
                    await _transferService.Send(request.Destination, new(request.FailedSegment.Sequencenumber, buffer, file.Id, request.Folder.Name));
                    _transferStore.DeleteFailedSegment(request.Destination, request.Folder, request.FailedSegment);
                }
                else
                {
                    //file not bound to folder, delete all failed items
                    _transferStore.DeleteFailedItemsOfFile(request.Destination, request.Folder, file);
                }
            }
            catch (Exception ex)
            {
                //do nothing.
                //todo log
            }

            return Unit.Value;
        }
    }
}
