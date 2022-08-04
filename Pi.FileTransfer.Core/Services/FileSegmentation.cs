using MediatR;
using Microsoft.Extensions.Logging;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
public class FileSegmentation : Segmentation
{
    private readonly IFileSystem _fileSystem;

    public FileSegmentation(IFileSystem fileSystem, ILogger<FileSegmentation> logger) : base(fileSystem,logger)
    {
        _fileSystem = fileSystem;
    }

    protected override Stream GetStream(Folder folder, Entities.File file) => _fileSystem.GetReadFileStream(file.GetFullPath(folder));
}
