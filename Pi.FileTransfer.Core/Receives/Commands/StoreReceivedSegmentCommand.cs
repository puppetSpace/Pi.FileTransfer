using MediatR;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.Receives.Commands;
public class StoreReceivedSegmentCommand : IRequest<Unit>
{
    public StoreReceivedSegmentCommand(Segment transferSegment)
    {
        TransferSegment = transferSegment;
    }

    public Segment TransferSegment { get; }

    internal class StoreReceivedSegmentCommandHandler : IRequestHandler<StoreReceivedSegmentCommand, Unit>
    {
        private readonly DataStore _dataStore;

        public StoreReceivedSegmentCommandHandler(DataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public async Task<Unit> Handle(StoreReceivedSegmentCommand request, CancellationToken cancellationToken)
        {
            await _dataStore.StoreReceivedSegment(request.TransferSegment);
            return Unit.Value;
        }
    }
}
