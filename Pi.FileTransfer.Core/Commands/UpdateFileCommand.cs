using MediatR;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class UpdateFileCommand : IRequest<Unit>
{
    public UpdateFileCommand(Entities.File file, Entities.Folder folder)
    {
        File = file;
        Folder = folder;
    }
    public Entities.File File { get; }

    public Entities.Folder Folder { get; }

    public class UpdateFileCommandHandler : IRequestHandler<UpdateFileCommand>
    {
        private readonly DeltaService _deltaService;

        public UpdateFileCommandHandler(DeltaService deltaService)
        {
            _deltaService = deltaService;
        }

        public Task<Unit> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {

        }
    }
}
