using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Events;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Commands;
public class RestartTransferFileCommand : IRequest<Unit>
{
	public RestartTransferFileCommand(Entities.File file, Folder folder, Destination destination, int lastPosition)
	{
		File = file;
		Folder = folder;
		Destination = destination;
		LastPosition = lastPosition;
	}

	public Entities.File File { get; }

	public Folder Folder { get; }
	public Destination Destination { get; }
	public int LastPosition { get; }


	private class RestartTransferFileCommandHandler : FileHandlingBase, IRequestHandler<RestartTransferFileCommand, Unit>
	{
		private readonly FileSegmentation _fileSegmentation;

		public RestartTransferFileCommandHandler(ILogger logger, TransferService transferService, DataStore dataStore, FileSegmentation fileSegmentation) : base(logger, transferService, dataStore, false)
		{
			_fileSegmentation = fileSegmentation;
		}

		//todo logging
		public async Task<Unit> Handle(RestartTransferFileCommand request, CancellationToken cancellationToken)
		{

			async Task SendSegmentForDestination(int sequenceNumber, byte[] buffer, Folder folder, Entities.File file)
			{
				await SendToDestination(sequenceNumber, buffer, folder, file, request.Destination);
			}

			var totalAmountOfSegments = await _fileSegmentation.Segment(request.Folder, request.File, request.LastPosition, SendSegmentForDestination);
			await SendReceipt(request.Destination, request.Folder, request.File, totalAmountOfSegments);
			DataStore.ClearLastPosition(request.Destination, request.Folder, request.File);

			return Unit.Value;
		}

		private async Task SendReceipt(Destination destination, Folder folder, Entities.File file, int totalAmountOfSegments)
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
