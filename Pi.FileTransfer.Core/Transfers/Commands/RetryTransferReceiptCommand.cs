using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Receives;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class RetryTransferReceiptCommand : IRequest<Unit>
{
    public RetryTransferReceiptCommand(FailedReceipt failedReceipt)
    {
        FailedReceipt = failedReceipt;
    }

    public FailedReceipt FailedReceipt { get; }

    public class RetryTransferReceiptCommandHandler : IRequestHandler<RetryTransferReceiptCommand>
    {
        private readonly TransferService _transferService;
        private readonly IFileTransferRepository _fileTransferRepository;
        private readonly ILogger<RetryTransferReceiptCommand> _logger;

        public RetryTransferReceiptCommandHandler(TransferService transferService, IFileTransferRepository fileTransferRepository, ILogger<RetryTransferReceiptCommand> logger)
        {
            _transferService = transferService;
            _fileTransferRepository = fileTransferRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(RetryTransferReceiptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.RetryTransferReceipt(request.FailedReceipt.File.GetFullPath(), request.FailedReceipt.Destination.Name);
                await _transferService.SendReceipt(request.FailedReceipt.Destination, new Receipt(request.FailedReceipt.File.Id, request.FailedReceipt.File.Folder.Name, request.FailedReceipt.File.RelativePath, request.FailedReceipt.TotalAmountOfSegments,request.FailedReceipt.File.Version, request.FailedReceipt.IsFileUpdate));
                _fileTransferRepository.RemoveFailedReceipt(request.FailedReceipt);
                await _fileTransferRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.FailedRetryTransferReceipt(request.FailedReceipt.File.GetFullPath(), request.FailedReceipt.Destination.Name, ex);
            }

            return Unit.Value;
        }
    }
}
