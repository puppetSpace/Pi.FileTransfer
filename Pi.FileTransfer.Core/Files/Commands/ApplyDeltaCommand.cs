﻿using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Receives;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Files.Commands;
public class ApplyDeltaCommand : IRequest<Unit>
{

    public ApplyDeltaCommand(Folder folder, Receipt transferReceipt, IEnumerable<Segment> transferSegments)
    {
        TransferReceipt = transferReceipt;
        Folder = folder;
        TransferSegments = transferSegments;
    }

    public Receipt TransferReceipt { get; }
    public Folder Folder { get; }
    public IEnumerable<Segment> TransferSegments { get; }

    internal class ApplyDeltaCommandHandler : AssembleCommandHandlerBase, IRequestHandler<ApplyDeltaCommand>
    {
        private readonly DataStore _dataStore;
        private readonly DeltaService _deltaService;
        private readonly IFolderRepository _folderRepository;

        public ApplyDeltaCommandHandler(DeltaSegmentation deltaSegmentation, DataStore dataStore, DeltaService deltaService
            , IFolderRepository folderRepository, ILogger<AssembleFileCommand> logger)
            : base(deltaSegmentation, logger)
        {
            _dataStore = dataStore;
            _deltaService = deltaService;
            _folderRepository = folderRepository;
        }

        public async Task<Unit> Handle(ApplyDeltaCommand request, CancellationToken cancellationToken)
        {
            //todo check if delta can be applied with version
            try
            {
                var targetPath = Path.Combine(request.Folder.FullName, request.TransferReceipt.RelativePath);
                if (!System.IO.File.Exists(targetPath))
                {
                    Logger.FileDoesNotExistForApplyingDelta(targetPath);
                    return Unit.Value;
                }

                var tempFile = await BuildFile(request.Folder, request.TransferReceipt, request.TransferSegments);
                var newLastWriteTime = _deltaService.ApplyDelta(tempFile, request.Folder, targetPath);

                var file = new Files.File(request.Folder
                    , request.TransferReceipt.FileId
                    , Path.GetExtension(request.TransferReceipt.RelativePath)
                    , request.TransferReceipt.RelativePath
                    , Path.GetFileName(request.TransferReceipt.RelativePath)
                    , newLastWriteTime
                    , request.TransferReceipt.Version);

                request.Folder.AddFile(file);
                _folderRepository.Update(request.Folder);
                await _folderRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                _dataStore.DeleteReceivedDataOfFile(request.Folder, request.TransferReceipt.FileId);
                //todo delete receipt
            }
            catch (Exception ex)
            {
                Logger.ApplyingDeltaFailed(request.TransferReceipt.RelativePath, ex);
            }


            return Unit.Value;
        }
    }

}
