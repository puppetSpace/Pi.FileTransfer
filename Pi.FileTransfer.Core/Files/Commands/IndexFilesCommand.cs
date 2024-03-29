﻿using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using System.IO;

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

                var toDelete = existingFiles.Where(x => !currentFiles.Any(y => x.GetFullPath() == y.file)).ToList();
                var toAdd = currentFiles.Where(x => !existingFiles.Any(y => x.file == y.GetFullPath())).ToList();
                var toUpdate = currentFiles.Where(x => existingFiles.Any(y => y.GetFullPath() == x.file && y.LastModified != x.lastModified)).ToList();

                foreach (var file in toDelete)
                {
                    _logger.RemoveFileFromIndex(file.RelativePath, request.Folder.Name);
                    request.Folder.RemoveFile(file);
                }

                foreach (var file in toAdd)
                {
                    _logger.AddFileToIndex(file.file, request.Folder.Name);
                    var buildFile = new Core.Files.File(request.Folder, Path.GetFileNameWithoutExtension(file.file)
                        , Path.GetExtension(file.file)
                        , file.file.Replace($"{request.Folder.FullName}{Path.DirectorySeparatorChar}", "")
                        , file.lastModified);

                    request.Folder.AddFile(buildFile);
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
