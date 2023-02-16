using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Files.Events;
using Pi.FileTransfer.Core.Files.Services;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using Pi.FileTransfer.Core.Transfers.Services;

namespace Pi.FileTransfer.Core.Transfers.Commands;
public class RestartTransferFileCommand : IRequest<Unit>
{
    public RestartTransferFileCommand(Files.File file, Folder folder, Destination destination, int readBytes)
    {
        File = file;
        Folder = folder;
        Destination = destination;
        ReadBytes = readBytes;
    }

    public Files.File File { get; }

    public Folder Folder { get; }
    public Destination Destination { get; }
    public int ReadBytes { get; }

    private class RestartTransferFileCommandHandler : FileHandlingBase, IRequestHandler<RestartTransferFileCommand, Unit>
    {
        private readonly FileSegmentation _fileSegmentation;

        public RestartTransferFileCommandHandler(ILogger<RestartTransferFileCommand> logger, TransferService transferService, IFileTransferRepository fileTransferRepository, FileSegmentation fileSegmentation) : base(logger, transferService, fileTransferRepository, false)
        {
            _fileSegmentation = fileSegmentation;
        }

        public async Task<Unit> Handle(RestartTransferFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                async Task SendSegmentForDestination(int sequenceNumber, byte[] buffer, Folder folder, Files.File file)
                {
                    await SendToDestination(sequenceNumber, buffer, file, request.Destination);
                }

                var totalAmountOfSegments = await _fileSegmentation.Segment(request.Folder, request.File, request.ReadBytes, SendSegmentForDestination);
                await SendReceipt(request.Destination, request.Folder, request.File, totalAmountOfSegments);
                await FileTransferRepository.ClearLastPosition(request.File, request.Destination);
            }
            catch (Exception ex)
            {
                Logger.FailedToRetryProcessOfFile(request.File.GetFullPath(),request.Destination.Name, ex);
            }
            finally
            {
                await FileTransferRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }

        private async Task SendReceipt(Destination destination, Folder folder, Files.File file, int totalAmountOfSegments)
        {
            try
            {
                Logger.SendReceipt(file.RelativePath, destination.Name);
                await TransferService.SendReceipt(destination, new(file.Id, folder.Name, file.RelativePath, totalAmountOfSegments,file.Version, IsFileUpdate));
            }
            catch (Exception ex)
            {
                Logger.SendReceiptFailed(file.RelativePath, destination.Name, ex);
                FileTransferRepository.AddFailedReceipt(new(file,destination, totalAmountOfSegments, IsFileUpdate));
            }
        }
    }
}
