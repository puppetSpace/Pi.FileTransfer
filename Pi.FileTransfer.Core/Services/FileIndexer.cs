using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
public class FileIndexer
{
	private readonly IFileSystem _fileSystem;

	public FileIndexer(IFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	public void IndexFiles(Entities.Folder folder)
	{
		var existingFiles = folder.Files;
		var currentFiles = _fileSystem.GetFiles(folder);

		var toDelete = existingFiles.Where(x => !currentFiles.Any(y => x.GetFullPath(folder) == y.file)).ToList();
		var toAdd = currentFiles.Where(x => !existingFiles.Any(y => x.file == y.GetFullPath(folder))).ToList();
		var toUpdate = existingFiles.Where(x => currentFiles.Any(y => x.GetFullPath(folder) == y.file && x.LastModified != y.lastModified)).ToList();

		foreach (var file in toDelete)
			folder.RemoveFile(file);

		foreach(var file in toAdd)
		{
			folder.AddFile(file.file,file.lastModified);
		}
	}
}
