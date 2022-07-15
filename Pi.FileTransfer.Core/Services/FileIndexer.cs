using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Core.Services;
public class FileIndexer
{
	private readonly IFileSystem _fileSystem;
	private readonly ILogger<FileIndexer> _logger;

	public FileIndexer(IFileSystem fileSystem, ILogger<FileIndexer> logger)
	{
		_fileSystem = fileSystem;
		_logger = logger;
	}

	public void IndexFiles(Entities.Folder folder)
	{
		var existingFiles = folder.Files;
		var currentFiles = _fileSystem.GetFiles(folder);

		var toDelete = existingFiles.Where(x => !currentFiles.Any(y => x.GetFullPath(folder) == y.file)).ToList();
		var toAdd = currentFiles.Where(x => !existingFiles.Any(y => x.file == y.GetFullPath(folder))).ToList();
		var toUpdate = currentFiles.Where(x => existingFiles.Any(y => y.GetFullPath(folder) == x.file && y.LastModified != x.lastModified)).ToList();

		foreach (var file in toDelete)
		{
			_logger.RemoveFileFromIndex(file.RelativePath, folder.Name);
			folder.RemoveFile(file);
		}

		foreach (var file in toAdd)
		{
			_logger.AddFileToIndex(file.file, folder.Name);
			folder.AddFile(file.file, file.lastModified);
		}

		foreach (var file in toUpdate)
		{
			_logger.UpdateFileInIndex(file.file, folder.Name);
			folder.UpdateFile(file.file, file.lastModified);
		}
	}
}
