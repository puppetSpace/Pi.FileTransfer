using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Common;

namespace Pi.FileTransfer.Core.Commands;
public class IndexFolderCommand : IRequest<Unit>
{
    public IndexFolderCommand(string path) => Path = path;

    public string Path { get; }

    public class IndexFolderCommandHandler : IRequestHandler<IndexFolderCommand>
    {
        private readonly ILogger<IndexFolderCommand> _logger;

        public IndexFolderCommandHandler(ILogger<IndexFolderCommand> logger)
        {
            _logger = logger;
        }

        public Task<Unit> Handle(IndexFolderCommand request, CancellationToken cancellationToken)
        {
            if (Directory.Exists(request.Path))
            {
                _logger.IndexingFolder(request.Path);
                var directory = new DirectoryInfo(System.IO.Path.Combine(request.Path, Constants.RootDirectoryName));
                directory.Create();
                directory.Attributes |= FileAttributes.Hidden;
            }
            return Unit.Task;
        }
    }
}
