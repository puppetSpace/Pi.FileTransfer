using MediatR;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Commands;
public class DeleteDestinationCommand : IRequest<Unit>
{
    public DeleteDestinationCommand(Guid destinationId)
    {
        DestinationId = destinationId;
    }

    public Guid DestinationId { get; }

    internal class DeleteDestinationCommandHandler : IRequestHandler<DeleteDestinationCommand, Unit>
    {
        private readonly IDestinationRepository _destinationRepository;

        public DeleteDestinationCommandHandler(IDestinationRepository destinationRepository)
        {
            _destinationRepository = destinationRepository;
        }

        public async Task<Unit> Handle(DeleteDestinationCommand request, CancellationToken cancellationToken)
        {
            if (request.DestinationId == Guid.Empty)
                return Unit.Value;

            await _destinationRepository.Delete(request.DestinationId);
            await _destinationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
