using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Receives;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.Files.Commands;
public class AssembleFileCommand : IRequest<Unit>
{

    public AssembleFileCommand(Folder folder, Receipt transferReceipt, IEnumerable<Segment> transferSegments)
    {
        TransferReceipt = transferReceipt;
        Folder = folder;
        TransferSegments = transferSegments;
    }

    public Receipt TransferReceipt { get; }
    public Folder Folder { get; }
    public IEnumerable<Segment> TransferSegments { get; }

    internal class AssembleFileCommandHandler : AssembleCommandHandlerBase, IRequestHandler<AssembleFileCommand>
    {
        private readonly DataStore _transferStore;
        private readonly IFileSystem _fileSystem;
        private readonly IFolderRepository _folderRepository;

        public AssembleFileCommandHandler(FileSegmentation fileSegmentation, DataStore transferStore
            , IFileSystem fileSystem, IFolderRepository folderRepository
            , ILogger<AssembleFileCommand> logger)
            : base(fileSegmentation, logger)
        {
            _transferStore = transferStore;
            _fileSystem = fileSystem;
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(AssembleFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tempFile = await BuildFile(request.Folder, request.TransferReceipt, request.TransferSegments);
                var destination = Path.Combine(request.Folder.FullName, request.TransferReceipt.RelativePath);
                _fileSystem.MoveFile(tempFile, destination);

                var file = new File(request.Folder
                    , request.TransferReceipt.FileId
                    , Path.GetExtension(request.TransferReceipt.RelativePath)
                    , request.TransferReceipt.RelativePath
                    , Path.GetFileName(request.TransferReceipt.RelativePath)
                    , new FileInfo(destination).LastWriteTimeUtc
                    , request.TransferReceipt.Version);
                request.Folder.AddFile(file);
                _folderRepository.Update(request.Folder);
                await _folderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                _transferStore.DeleteReceivedDataOfFile(request.Folder, request.TransferReceipt.FileId);
            }
            catch (Exception ex)
            {
                Logger.BuildingFileFailed(request.TransferReceipt.RelativePath, ex);
            }


            return Unit.Value;
        }
    }

}
