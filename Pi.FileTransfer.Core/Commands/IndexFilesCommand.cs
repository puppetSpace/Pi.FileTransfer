using MediatR;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class IndexFilesCommand : IRequest<Unit>
{

    public IndexFilesCommand(Folder folder)
    {
        Folder = folder;
    }

    public Folder Folder { get; }


    public class IndexFilesCommandHandler : IRequestHandler<IndexFilesCommand>
    {
        private readonly FileIndexer _fileIndexer;

        public IndexFilesCommandHandler(FileIndexer fileIndexer)
        {
            _fileIndexer = fileIndexer;
        }

        public Task<Unit> Handle(IndexFilesCommand request, CancellationToken cancellationToken)
        {
            _fileIndexer.IndexFiles(request.Folder);
            return Unit.Task;
        }
    }
}
