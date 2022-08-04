using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class UpdateFileCommand : IRequest<Unit>
{
    public UpdateFileCommand(Entities.File file, Entities.Folder folder)
    {
        File = file;
        Folder = folder;
    }
    public Entities.File File { get; }

    public Entities.Folder Folder { get; }

    public class UpdateFileCommandHandler : FileCommandHandlerBase, IRequestHandler<UpdateFileCommand>
    {
        private readonly DeltaService _deltaService;
        private readonly DeltaSegmentation _deltaSegmentation;
        private readonly ILogger<UpdateFileCommand> _logger;

        public UpdateFileCommandHandler(DeltaService deltaService, DeltaSegmentation deltaSegmentation
            , TransferService transferService,DataStore dataStore, ILogger<UpdateFileCommand> logger)
            :base(logger,transferService,dataStore,true)
        {
            _deltaService = deltaService;
            _deltaSegmentation = deltaSegmentation;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {
            if (!request.Folder.Destinations.Any())
                return Unit.Value;
            
            _logger.ProcessUpdatedFile(request.File.RelativePath);
            
            await _deltaService.CreateDelta(request.Folder, request.File);
            _deltaService.CreateSignature(request.Folder, request.File);
 
            var totalAmountOfSegments = await _deltaSegmentation.Segment(request.Folder, request.File, SendSegment);
            await SendReceipt(request.Folder, request.File, totalAmountOfSegments);

            return Unit.Value;
        }

        private async Task SendReceipt(Folder folder, Entities.File file, int totalAmountOfSegments)
        {
            foreach (var destination in folder.Destinations)
            {
                try
                {
                    Logger.SendReceipt(file.RelativePath, destination.Name);
                    await TransferService.SendReceipt(destination, new(file.Id, file.RelativePath, totalAmountOfSegments, folder.Name, IsFileUpdate));
                }
                catch (Exception ex)
                {
                    Logger.SendReceiptFailed(file.RelativePath, destination.Name, ex);
                    await DataStore.StoreFailedReceiptTransfer(destination, folder, file, totalAmountOfSegments, IsFileUpdate);
                }
            }
        }
    }
}
