using MediatR;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class StoreReceivedReceiptCommand : IRequest<Unit>
{
    public StoreReceivedReceiptCommand(TransferReceipt transferReceipt)
    {
        TransferReceipt = transferReceipt;
    }

    public TransferReceipt TransferReceipt { get; }


    public class StoreReceivedReceiptCommandHandler : IRequestHandler<StoreReceivedReceiptCommand, Unit>
    {
        private readonly DataStore _dataStore;

        public StoreReceivedReceiptCommandHandler(DataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public async Task<Unit> Handle(StoreReceivedReceiptCommand request, CancellationToken cancellationToken)
        {
            await _dataStore.StoreReceivedReceipt(request.TransferReceipt);
            return Unit.Value;
        }
    }
}
