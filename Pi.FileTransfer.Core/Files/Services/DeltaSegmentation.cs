﻿using Microsoft.Extensions.Logging;
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
    private readonly DataStore _dataStore;

    public DeltaSegmentation(IFileSystem fileSystem, DataStore dataStore, IOptions<AppSettings> options, ILogger<FileSegmentation> logger) : base(fileSystem, dataStore, options, logger)
    {
        _fileSystem = fileSystem;
        _dataStore = dataStore;
    }

    protected override Stream GetStream(Folder folder, File file) => _fileSystem.GetReadFileStream(_dataStore.GetDeltaFilePath(folder, file));
}