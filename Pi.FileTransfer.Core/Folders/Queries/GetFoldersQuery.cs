using MediatR;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Core.Folders.Queries;
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
