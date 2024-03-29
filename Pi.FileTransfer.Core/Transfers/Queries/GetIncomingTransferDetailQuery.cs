﻿using MediatR;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;

namespace Pi.FileTransfer.Core.Transfers.Queries;
public class GetIncomingTransferDetailQuery : IRequest<IncomingTransferDetail>
{
    public GetIncomingTransferDetailQuery(string folderName)
    {
        FolderName = folderName;
    }

    public string FolderName { get; }


    internal class GetIncomingTransferDetailQueryHandler : IRequestHandler<GetIncomingTransferDetailQuery, IncomingTransferDetail>
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
            //todo use query
            //var folder = await _folderRepository.GetFolder(request.FolderName);
            //if (folder == Folder.Empty)
            //    return new IncomingTransferDetail(0, 0);
            //return new IncomingTransferDetail(_dataStore.GetReceivedSegmentsCount(folder), _dataStore.GetReceivedReceiptsCount(folder));
            return new IncomingTransferDetail(0, 0);

        }
    }
}


public record IncomingTransferDetail(int AmountOfSegments, int AmountOfReceipts);