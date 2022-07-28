using MediatR;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Queries;
public class GetFoldersQuery : IRequest<IEnumerable<Folder>>
{


    public class GetFoldersQueryHandler : IRequestHandler<GetFoldersQuery, IEnumerable<Folder>>
    {
        private readonly IFolderRepository _folderRepository;

        public GetFoldersQueryHandler(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        public async Task<IEnumerable<Folder>> Handle(GetFoldersQuery request, CancellationToken cancellationToken)
        {
            var result = await _folderRepository.GetFolders().ToListAsync(cancellationToken: cancellationToken);
            return result;
        }
    }
}
