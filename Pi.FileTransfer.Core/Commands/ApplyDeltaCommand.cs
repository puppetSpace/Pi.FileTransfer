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
public class ApplyDeltaCommand : IRequest<Unit>
{

    public ApplyDeltaCommand(Folder folder,TransferReceipt transferReceipt, IEnumerable<TransferSegment> transferSegments)
    {
        TransferReceipt = transferReceipt;
        Folder = folder;
        TransferSegments = transferSegments;
    }

    public TransferReceipt TransferReceipt { get; }
    public Folder Folder { get; }
    public IEnumerable<TransferSegment> TransferSegments { get; }

    public class ApplyDeltaCommandHandler : AssembleCommandHandlerBase, IRequestHandler<ApplyDeltaCommand>
    {
        private readonly DataStore _dataStore;
        private readonly DeltaService _deltaService;
        private readonly IFolderRepository _folderRepository;

        public ApplyDeltaCommandHandler(DeltaSegmentation deltaSegmentation, DataStore dataStore, DeltaService deltaService
            , IFolderRepository folderRepository, ILogger<AssembleFileCommand> logger)
            :base(deltaSegmentation,logger)
        {
            _dataStore = dataStore;
            _deltaService = deltaService;
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(ApplyDeltaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tempFile = await base.BuildFile(request.Folder, request.TransferReceipt, request.TransferSegments);
                var destination = Path.Combine(request.Folder.FullName, request.TransferReceipt.RelativePath);
                if (!System.IO.File.Exists(destination))
                {
                    Logger.FileDoesNotExistForApplyingDelta(destination);
                    return Unit.Value;
                }

                var newLastWriteTime = _deltaService.ApplyDelta(tempFile, request.Folder, destination);

                var file = new Entities.File
                {
                    Id = request.TransferReceipt.FileId,
                    Extension = Path.GetExtension(request.TransferReceipt.RelativePath),
                    RelativePath = request.TransferReceipt.RelativePath,
                    Name = Path.GetFileName(request.TransferReceipt.RelativePath),
                    LastModified = newLastWriteTime,
                };
                request.Folder.AddFile(file);
                await _folderRepository.Save(request.Folder);
                _dataStore.DeleteReceivedDataOfFile(request.Folder, request.TransferReceipt.FileId);
            }
            catch (Exception ex)
            {
                Logger.ApplyingDeltaFailed(request.TransferReceipt.RelativePath, ex);
            }


            return Unit.Value;
        }
    }

}
