using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Core.Folders.Commands;
public class IndexFolderCommand : IRequest<Unit>
{
    public IndexFolderCommand(string path) => Path = path;

    public string Path { get; }

    public class IndexFolderCommandHandler : IRequestHandler<IndexFolderCommand>
    {
        private readonly ILogger<IndexFolderCommand> _logger;
        private readonly IOptions<AppSettings> _options;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileSystem _fileSystem;

        public IndexFolderCommandHandler(ILogger<IndexFolderCommand> logger, IOptions<AppSettings> options, IFolderRepository folderRepository, IFileSystem fileSystem)
        {
            _logger = logger;
            _options = options;
            _folderRepository = folderRepository;
            _fileSystem = fileSystem;
        }

        public async Task<Unit> Handle(IndexFolderCommand request, CancellationToken cancellationToken)
        {
            _fileSystem.CreateDirectory(request.Path);

            var folderName = System.IO.Path.GetFileName(request.Path);
            var folder = await _folderRepository.Get(folderName);
            if (folder is null)
            {
                _logger.IndexingFolder(request.Path);

                _folderRepository.Add(new Folder(Guid.NewGuid(), folderName, System.IO.Path.Combine(_options.Value.BasePath, folderName), new List<Core.Files.File>(), new List<Destination>()));
                CreateSyncFolder(request.Path);
                CreateIncomingFolder(request.Path);
                CreateIncomingTempFolder(request.Path);
                CreateDeltaFolderFolder(request.Path);
                CreateSignatureFolder(request.Path);
                await _folderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }

        private static void CreateSyncFolder(string folderpath)
        {
            var directory = new DirectoryInfo(System.IO.Path.Combine(folderpath, Constants.RootDirectoryName));
            directory.Create();
            directory.Attributes |= FileAttributes.Hidden;
        }

        private void CreateIncomingFolder(string folderPath)
            => _fileSystem.CreateDirectory(FolderUtils.GetIncomingFolderPath(folderPath));

        private void CreateIncomingTempFolder(string folderPath)
            => _fileSystem.CreateDirectory(FolderUtils.GetIncomingFolderTempPath(folderPath));

        private void CreateDeltaFolderFolder(string folderPath) 
            => _fileSystem.CreateDirectory(FolderUtils.GetDeltasFolderPath(folderPath));

        private void CreateSignatureFolder(string folderPath) 
            => _fileSystem.CreateDirectory(FolderUtils.GetSignaturesFolderPath(folderPath));
    }
}
