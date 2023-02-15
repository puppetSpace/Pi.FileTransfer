using MediatR;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.Receives.Commands;
public class StoreReceivedReceiptCommand : IRequest<Unit>
{
    public StoreReceivedReceiptCommand(Receipt transferReceipt)
    {
        TransferReceipt = transferReceipt;
    }

    public Receipt TransferReceipt { get; }


    public class StoreReceivedReceiptCommandHandler : IRequestHandler<StoreReceivedReceiptCommand, Unit>
    {
        private readonly IFileReceiveRepository _fileReceiveRepository;

        public StoreReceivedReceiptCommandHandler(IFileReceiveRepository fileReceiveRepository)
        {
            _fileReceiveRepository = fileReceiveRepository;
        }

        public async Task<Unit> Handle(StoreReceivedReceiptCommand request, CancellationToken cancellationToken)
        {
            _fileReceiveRepository.AddReceipt(request.TransferReceipt);
            await _fileReceiveRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
