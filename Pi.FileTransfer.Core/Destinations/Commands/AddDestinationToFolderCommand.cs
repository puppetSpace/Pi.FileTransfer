using MediatR;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Commands;
public class AddDestinationToFolderCommand : IRequest<Unit>
{
    public AddDestinationToFolderCommand(string folder, Guid destinationId)
    {
        Folder = folder;
        DestinationId = destinationId;
    }

    public string Folder { get; }
    public Guid DestinationId { get; }

    internal class AddDestinationToFolderCommandHandler : IRequestHandler<AddDestinationToFolderCommand, Unit>
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IDestinationRepository _destinationRepository;

        public AddDestinationToFolderCommandHandler(IFolderRepository folderRepository, IDestinationRepository destinationRepository)
        {
            _folderRepository = folderRepository;
            _destinationRepository = destinationRepository;
        }

        public async Task<Unit> Handle(AddDestinationToFolderCommand request, CancellationToken cancellationToken)
        {
            if (request.DestinationId == Guid.Empty || string.IsNullOrWhiteSpace(request.Folder))
                return Unit.Value;

            var folder = await _folderRepository.Get(request.Folder);
            var destination = await _destinationRepository.Get(request.DestinationId);
            if (folder is not null && destination is not null)
            {
                folder.AddDestination(destination);
                _folderRepository.Add(folder);
                await _folderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }
    }
}
