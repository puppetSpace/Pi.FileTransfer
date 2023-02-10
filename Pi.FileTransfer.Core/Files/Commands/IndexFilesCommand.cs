using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;

namespace Pi.FileTransfer.Core.Files.Commands;
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
        private readonly ILogger<IndexFilesCommand> _logger;

        public IndexFilesCommandHandler(FileIndexer fileIndexer, ILogger<IndexFilesCommand> logger)
        {
            _fileIndexer = fileIndexer;
            _logger = logger;
        }

        public Task<Unit> Handle(IndexFilesCommand request, CancellationToken cancellationToken)
        {
            _logger.IndexingFiles(request.Folder.Name);
            _fileIndexer.IndexFiles(request.Folder);
            return Unit.Task;
        }
    }
}
