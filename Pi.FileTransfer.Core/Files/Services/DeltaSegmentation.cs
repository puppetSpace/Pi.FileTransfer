using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Files.Services;
public class DeltaSegmentation : Segmentation
{
    private readonly IFileSystem _fileSystem;

    public DeltaSegmentation(IFileSystem fileSystem, IOptions<AppSettings> options, ILogger<FileSegmentation> logger) : base(fileSystem, options, logger)
    {
        _fileSystem = fileSystem;
    }

    protected override Stream GetStream(File file) => _fileSystem.GetReadFileStream(FolderUtils.GetDeltasFolderPath(file.Folder.Name,file.Name));
}
