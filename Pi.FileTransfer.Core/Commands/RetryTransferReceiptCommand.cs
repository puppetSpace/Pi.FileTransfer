using MediatR;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class RetryTransferReceiptCommand : IRequest<Unit>
{
    public RetryTransferReceiptCommand(FailedReceipt failedReceipt, Destination destination, Folder folder)
    {
        FailedReceipt = failedReceipt;
        Destination = destination;
        Folder = folder;
    }

    public FailedReceipt FailedReceipt { get; }

    public Destination Destination { get; }

    public Folder Folder { get; }

    public class RetryTransferReceiptCommandHandler : IRequestHandler<RetryTransferReceiptCommand>
    {
        private readonly TransferService _transferService;
        private readonly DataStore _transferStore;

        public RetryTransferReceiptCommandHandler(TransferService transferService, DataStore transferStore)
        {
            _transferService = transferService;
            _transferStore = transferStore;
        }

        public async Task<Unit> Handle(RetryTransferReceiptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var file = request.Folder.Files.SingleOrDefault(x => x.Id == request.FailedReceipt.FileId);
                if (file is not null)
                {
                    await _transferService.SendReceipt(request.Destination, new TransferReceipt(request.FailedReceipt.FileId, file.RelativePath, request.FailedReceipt.TotalAmountOfSegments, request.Folder.Name));
                    _transferStore.DeleteFailedReceipt(request.Destination, request.Folder, request.FailedReceipt);
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
