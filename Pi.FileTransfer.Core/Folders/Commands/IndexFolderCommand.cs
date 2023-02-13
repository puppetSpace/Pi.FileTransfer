using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Common;

namespace Pi.FileTransfer.Core.Folders.Commands;
public class IndexFolderCommand : IRequest<Unit>
{
    public IndexFolderCommand(string path) => Path = path;

    public string Path { get; }

    public class IndexFolderCommandHandler : IRequestHandler<IndexFolderCommand>
    {
        private readonly ILogger<IndexFolderCommand> _logger;

        public IndexFolderCommandHandler(ILogger<IndexFolderCommand> logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(IndexFolderCommand request, CancellationToken cancellationToken)
        {
            if (Directory.Exists(request.Path))
            {
                _logger.IndexingFolder(request.Path);
                CreateSyncFolder(request.Path);
                CreateIncomingFolder(request.Path);
                CreateIncomingTempFolder(request.Path);
                CreateDeltaFolderFolder(request.Path);
                CreateSignatureFolder(request.Path);
            }
            return Unit.Task;
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
