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

    public class AssembleFileCommandHandler : IRequestHandler<AssembleFileCommand>
    {
        private readonly FileSegmentation _fileSegmentation;
        private readonly DataStore _transferStore;
        private readonly IFileSystem _fileSystem;
        private readonly IFolderRepository _folderRepository;
        private readonly ILogger<AssembleFileCommand> _logger;

        public AssembleFileCommandHandler(FileSegmentation fileSegmentation, DataStore transferStore
            , IFileSystem fileSystem, IFolderRepository folderRepository
            , ILogger<AssembleFileCommand> logger)
        {
            _fileSegmentation = fileSegmentation;
            _transferStore = transferStore;
            _fileSystem = fileSystem;
            _folderRepository = folderRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(AssembleFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.BuildingFile(request.TransferReceipt.RelativePath);
                var tempFile = await _fileSegmentation.BuildFile(request.Folder, request.TransferReceipt.FileId, request.TransferSegments.OrderBy(x => x.Sequencenumber).Select(x => x.Buffer));
                var destination = Path.Combine(request.Folder.FullName, request.TransferReceipt.RelativePath);
                _fileSystem.MoveFile(tempFile, destination);

                var file = new Entities.File
                {
                    Id = request.TransferReceipt.FileId,
                    Extension = Path.GetExtension(request.TransferReceipt.RelativePath),
                    RelativePath = request.TransferReceipt.RelativePath,
                    Name = Path.GetFileName(request.TransferReceipt.RelativePath),
                    LastModified = new FileInfo(destination).LastWriteTimeUtc
                };
                request.Folder.AddFile(file);
                await _folderRepository.Save(request.Folder);
                _transferStore.DeleteReceivedDataOfFile(request.Folder, request.TransferReceipt.FileId);
            }
            catch (Exception ex)
            {
                _logger.BuildingFileFailed(request.TransferReceipt.RelativePath, ex);
            }


            return Unit.Value;
        }
    }

}
