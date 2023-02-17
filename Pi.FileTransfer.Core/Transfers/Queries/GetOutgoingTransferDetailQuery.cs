using MediatR;
using Pi.FileTransfer.Core;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Transfers.Queries;
public class GetOutgoingTransferDetailQuery : IRequest<IEnumerable<OutgoingTransferDetail>>
{
    public GetOutgoingTransferDetailQuery(string folderName) => FolderName = folderName;
    public string FolderName { get; }

    public class GetTransferDetailQueryHandler : IRequestHandler<GetOutgoingTransferDetailQuery, IEnumerable<OutgoingTransferDetail>>
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFileTransferRepository _fileTransferRepository;

        public GetTransferDetailQueryHandler(IFolderRepository folderRepository, IFileTransferRepository fileTransferRepository)
        {
            _folderRepository = folderRepository;
            _fileTransferRepository = fileTransferRepository;
        }

        public async Task<IEnumerable<OutgoingTransferDetail>> Handle(GetOutgoingTransferDetailQuery request, CancellationToken cancellationToken)
        {
            //todo change this with a query
            var folder = await _folderRepository.Get(request.FolderName);
            if (folder == Folder.Empty)
                return new List<OutgoingTransferDetail>();
            var details = new List<OutgoingTransferDetail>();
            foreach (var destination in folder.Destinations)
            {
                var failedSegments = await _fileTransferRepository.GetFailedSegments(folder, destination);
                var failedReceipts = await _fileTransferRepository.GetFailedReceipts(folder, destination);
                foreach (var file in folder.Files)
                {
                    var lastPosition = await _fileTransferRepository.GetLastPosition(file,destination);
                    details.Add(new(file.Id, destination.Name, lastPosition?.ReadBytes, failedSegments.Where(x => x.File.Id == file.Id).ToList(), failedReceipts.Any(x => x.File.Id == file.Id)));
                }
            }
            return details;
        }
    }

}

public record OutgoingTransferDetail(Guid FileId, string Destination, int? ReadTill, List<FailedSegment> FailedSegments, bool HasFailedReceipt);
