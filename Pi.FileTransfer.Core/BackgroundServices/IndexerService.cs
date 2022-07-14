﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Commands;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.BackgroundServices;
public class IndexerService : BackgroundService
{
    private readonly IFileSystem _fileSystem;
    private readonly IFolderRepository _folderRepository;
    private readonly IMediator _mediator;

    public IndexerService(IFileSystem fileSystem, IFolderRepository folderRepository, IMediator mediator)
    {
        _fileSystem = fileSystem;
        _folderRepository = folderRepository;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await IndexFolders();
            await IndexFiles();

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task IndexFolders()
    {
        var foldersWithoutSyncFolder = _fileSystem.GetFoldersFromBasePath()
            .Where(x => !Directory.Exists(Path.Combine(x, Constants.RootDirectoryName)));
        foreach (var folder in foldersWithoutSyncFolder)
        {
            await _mediator.Send(new IndexFolderCommand(folder));
        }
    }

    private async Task IndexFiles()
    {
        var fileIndexer = new FileIndexer(_fileSystem);

        await foreach (var folder in _folderRepository.GetFolders())
        {
            fileIndexer.IndexFiles(folder);
            await _folderRepository.Save(folder);
        }
    }
}
