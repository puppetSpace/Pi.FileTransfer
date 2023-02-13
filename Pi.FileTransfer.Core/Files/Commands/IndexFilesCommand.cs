using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;

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
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<IndexFilesCommand> _logger;

        public IndexFilesCommandHandler(IFileSystem fileSystem, ILogger<IndexFilesCommand> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public Task<Unit> Handle(IndexFilesCommand request, CancellationToken cancellationToken)
        {
            _logger.IndexingFiles(request.Folder.Name);

            try
            {
                var existingFiles = request.Folder.Files;
                var currentFiles = _fileSystem.GetFiles(request.Folder);

                var toDelete = existingFiles.Where(x => !currentFiles.Any(y => x.GetFullPath(request.Folder) == y.file)).ToList();
                var toAdd = currentFiles.Where(x => !existingFiles.Any(y => x.file == y.GetFullPath(request.Folder))).ToList();
                var toUpdate = currentFiles.Where(x => existingFiles.Any(y => y.GetFullPath(request.Folder) == x.file && y.LastModified != x.lastModified)).ToList();

                foreach (var file in toDelete)
                {
                    _logger.RemoveFileFromIndex(file.RelativePath, request.Folder.Name);
                    request.Folder.RemoveFile(file);
                }

                foreach (var file in toAdd)
                {
                    _logger.AddFileToIndex(file.file, request.Folder.Name);
                    request.Folder.AddFile(file.file, file.lastModified);
                }

                foreach (var file in toUpdate)
                {
                    _logger.UpdateFileInIndex(file.file, request.Folder.Name);
                    request.Folder.UpdateFile(file.file, file.lastModified);
                }
            }
            catch (Exception ex)
            {
                _logger.FailedToIndexFiles(request.Folder.Name, ex);
            }

            return Unit.Task;
        }
    }
}
