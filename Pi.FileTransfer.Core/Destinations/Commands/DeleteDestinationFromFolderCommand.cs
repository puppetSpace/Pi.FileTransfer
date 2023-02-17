using MediatR;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Destinations.Commands;
public class DeleteDestinationFromFolderCommand : IRequest<Unit>
{
    public DeleteDestinationFromFolderCommand(string folder, string destination)
    {
        Folder = folder;
        Destination = destination;
    }

    public string Folder { get; }
    public string Destination { get; }


    public class DeleteDestinationFromFolderCommandHandler : IRequestHandler<DeleteDestinationFromFolderCommand, Unit>
    {
        private readonly IFolderRepository _folderRepository;

        public DeleteDestinationFromFolderCommandHandler(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(DeleteDestinationFromFolderCommand request, CancellationToken cancellationToken)
        {
            var folder = await _folderRepository.Get(request.Folder);
            if (folder is not null)
            {
                folder.DeleteDestination(request.Destination);
                _folderRepository.Update(folder);
                await _folderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
