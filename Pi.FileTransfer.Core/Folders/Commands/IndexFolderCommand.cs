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

        public IndexFolderCommandHandler(ILogger<IndexFolderCommand> logger, IOptions<AppSettings> options, IFolderRepository folderRepository)
        {
            _logger = logger;
            _options = options;
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(IndexFolderCommand request, CancellationToken cancellationToken)
        {
            if (Directory.Exists(request.Path))
            {
                _logger.IndexingFolder(request.Path);

                var folderName = System.IO.Path.GetFileName(request.Path);
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

        private static void CreateIncomingFolder(string folderPath)
            => System.IO.Directory.CreateDirectory(FolderUtils.GetIncomingFolderPath(folderPath));

        private static void CreateIncomingTempFolder(string folderPath)
            => System.IO.Directory.CreateDirectory(FolderUtils.GetIncomingFolderTempPath(folderPath));

        private static void CreateDeltaFolderFolder(string folderPath)
            => System.IO.Directory.CreateDirectory(FolderUtils.GetDeltasFolderPath(folderPath));

        private static void CreateSignatureFolder(string folderPath)
            => System.IO.Directory.CreateDirectory(FolderUtils.GetSignaturesFolderPath(folderPath));
    }
}
