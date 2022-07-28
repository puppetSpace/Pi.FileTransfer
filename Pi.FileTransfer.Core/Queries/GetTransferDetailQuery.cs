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
public class GetTransferDetailQuery : IRequest<IEnumerable<TransferDetail>>
{
    public GetTransferDetailQuery(string folderName) => FolderName = folderName;
    public string FolderName { get;}

    public class GetTransferDetailQueryHandler : IRequestHandler<GetTransferDetailQuery, IEnumerable<TransferDetail>>
    {
        private readonly IFolderRepository _folderRepository;
        private readonly DataStore _dataStore;

        public GetTransferDetailQueryHandler(IFolderRepository folderRepository, DataStore dataStore)
        {
            _folderRepository = folderRepository;
            _dataStore = dataStore;
        }

        public async Task<IEnumerable<TransferDetail>> Handle(GetTransferDetailQuery request, CancellationToken cancellationToken)
        {
            var folder = await _folderRepository.GetFolder(request.FolderName);
            if(folder == Folder.Empty)
                return new List<TransferDetail>();
            var details = new List<TransferDetail>();
            foreach (var destination in folder.Destinations)
            {
                var failedSegments = await _dataStore.GetFailedSegments(folder, destination).ToListAsync(cancellationToken: cancellationToken);
                var failedReceipts = await _dataStore.GetFailedReceipts(folder, destination).ToListAsync(cancellationToken: cancellationToken);
                foreach(var file in folder.Files)
                {
                    var readTill = await _dataStore.GetLastPosition(folder, destination, file.Id);
                    details.Add(new(file.Id, destination.Name, readTill, failedSegments.Where(x => x.FileId == file.Id).ToList(), failedReceipts.Any(x => x.FileId == file.Id)));
                }
            }
            return details;
        }
    }

}

public record TransferDetail(Guid FileId, string Destination, int? ReadTill, List<FailedSegment> FailedSegments, bool HasFailedReceipt);
