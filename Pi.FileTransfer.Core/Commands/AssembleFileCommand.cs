using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class AssembleFileCommand : IRequest<Unit>
{

    public AssembleFileCommand(Folder folder,TransferReceipt transferReceipt, IEnumerable<TransferSegment> transferSegments)
    {
        TransferReceipt = transferReceipt;
        Folder = folder;
        TransferSegments = transferSegments;
    }

    public TransferReceipt TransferReceipt { get; }
    public Folder Folder { get; }
    public IEnumerable<TransferSegment> TransferSegments { get; }

    public class AssembleFileCommandHandler : AssembleCommandHandlerBase, IRequestHandler<AssembleFileCommand>
    {
        private readonly DataStore _transferStore;
        private readonly IFileSystem _fileSystem;
        private readonly IFolderRepository _folderRepository;

        public AssembleFileCommandHandler(FileSegmentation fileSegmentation, DataStore transferStore
            , IFileSystem fileSystem, IFolderRepository folderRepository
            , ILogger<AssembleFileCommand> logger)
            :base(fileSegmentation,logger)
        {
            _transferStore = transferStore;
            _fileSystem = fileSystem;
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(AssembleFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tempFile = await base.BuildFile(request.Folder, request.TransferReceipt, request.TransferSegments);
                var destination = Path.Combine(request.Folder.FullName, request.TransferReceipt.RelativePath);
                _fileSystem.MoveFile(tempFile, destination);

                var file = new Entities.File(request.TransferReceipt.FileId
                    , Path.GetExtension(request.TransferReceipt.RelativePath)
                    , request.TransferReceipt.RelativePath
                    , Path.GetFileName(request.TransferReceipt.RelativePath)
                    , new FileInfo(destination).LastWriteTimeUtc);
                request.Folder.AddFile(file);
                await _folderRepository.Save(request.Folder);
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
