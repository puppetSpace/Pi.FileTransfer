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
    public AddDestinationToFolderCommand(string folder, Destination destination)
    {
        Folder = folder;
        Destination = destination;
    }

    public string Folder { get; }
    public Destination Destination { get; }

    internal class AddDestinationToFolderCommandHandler : IRequestHandler<AddDestinationToFolderCommand, Unit>
    {
        private readonly IFolderRepository _folderRepository;

        public AddDestinationToFolderCommandHandler(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(AddDestinationToFolderCommand request, CancellationToken cancellationToken)
        {
            if (request.Destination is null || string.IsNullOrWhiteSpace(request.Folder))
                return Unit.Value;

            var folder = await _folderRepository.GetFolder(request.Folder);
            if (folder is not null)
            {
                folder.AddDestination(request.Destination);
                _folderRepository.Add(folder);
                await _folderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }
    }
}
