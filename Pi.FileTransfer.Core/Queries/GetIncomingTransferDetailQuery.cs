using MediatR;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Queries;
public class GetIncomingTransferDetailQuery : IRequest<IncomingTransferDetail>
{
	public GetIncomingTransferDetailQuery(string folderName)
	{
		FolderName = folderName;
	}

	public string FolderName { get; }


	public class GetIncomingTransferDetailQueryHandler : IRequestHandler<GetIncomingTransferDetailQuery, IncomingTransferDetail>
	{
		private readonly IFolderRepository _folderRepository;
		private readonly DataStore _dataStore;

		public GetIncomingTransferDetailQueryHandler(IFolderRepository folderRepository, DataStore dataStore)
		{
			_folderRepository = folderRepository;
			_dataStore = dataStore;
		}

		public async Task<IncomingTransferDetail> Handle(GetIncomingTransferDetailQuery request, CancellationToken cancellationToken)
		{
            var folder = await _folderRepository.GetFolder(request.FolderName);
            if (folder == Folder.Empty)
                return new IncomingTransferDetail(0,0);
            return new IncomingTransferDetail(_dataStore.GetReceivedSegmentsCount(folder),_dataStore.GetReceivedReceiptsCount(folder));
        }
	}
}


public record IncomingTransferDetail(int AmountOfSegments, int AmountOfReceipts);