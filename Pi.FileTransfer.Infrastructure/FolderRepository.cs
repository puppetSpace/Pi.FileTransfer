using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Entities;
using Pi.FileTransfer.Core.Interfaces;
using System.Text.Json;

namespace Pi.FileTransfer.Infrastructure;
public class FolderRepository : IFolderRepository
{
    private readonly string _basePath;
    private readonly IMediator _mediator;
    private static readonly SemaphoreSlim _locker = new(1);
    private static readonly SemaphoreSlim _lockerDestinations = new(1);

    public FolderRepository(IOptions<AppSettings> options, IMediator mediator)
    {
        _basePath = options.Value.BasePath;
        _mediator = mediator;
    }

    public async IAsyncEnumerable<Folder> GetFolders()
    {
        var syncFolders = Directory.GetDirectories(_basePath);
        foreach (var folder in syncFolders)
        {
            if (Directory.Exists(Path.Combine(folder, Constants.RootDirectoryName)))
                yield return await BuildFolderEntityAsync(folder);
        }
    }

    public async Task<Folder> GetFolder(string name)
    {
        var syncFolder = Directory.GetDirectories(_basePath).FirstOrDefault(x => x.EndsWith(name, StringComparison.OrdinalIgnoreCase));
        if (syncFolder is null)
            return Folder.Empty;

        return await BuildFolderEntityAsync(syncFolder);
    }

    public async Task Save(Folder folder)
    {
        await SaveFileIndex(folder);
        await SaveDestinations(folder);

        foreach (var @event in folder.Events)
            _ = _mediator.Publish(@event);
    }

    private static async Task SaveFileIndex(Folder folder)
    {
        try
        {
            await _locker.WaitAsync();
            var indexFile = Path.Combine(folder.FullName, Constants.RootDirectoryName, Constants.IndexFileName);
            using var fs = new FileStream(indexFile, FileMode.OpenOrCreate, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fs, folder.Files.ToList());
        }
        finally
        {
            _locker.Release();
        }
    }
    private static async Task SaveDestinations(Folder folder)
    {
        var destinationsFile = Path.Combine(folder.FullName, Constants.RootDirectoryName, Constants.DestinationsFileName);
        await _lockerDestinations.WaitAsync();
        try
        {
            using var fs = System.IO.File.OpenWrite(destinationsFile);
            await JsonSerializer.SerializeAsync(fs, folder.Destinations);
        }
        finally
        {
            _lockerDestinations.Release();
        }
    }
    private static async Task<List<Destination>> GetDestinations(string folder)
    {
        var destinationsFile = Path.Combine(folder, Constants.RootDirectoryName, Constants.DestinationsFileName);
        if (System.IO.File.Exists(destinationsFile))
        {
            await _lockerDestinations.WaitAsync();
            try
            {
                using var fs = System.IO.File.OpenRead(destinationsFile);
                return (await JsonSerializer.DeserializeAsync<List<Destination>>(fs)) ?? new();
            }
            finally
            {
                _lockerDestinations.Release();
            }
        }
        else
        {
            return new();
        }
    }

    private async Task<Folder> BuildFolderEntityAsync(string folder)
    {
        var folderName = Path.GetFileName(folder);

        return new(folderName, Path.Combine(_basePath, folderName), await GetFiles(folder), await GetDestinations(folder));
    }

    private async Task<List<Core.Entities.File>> GetFiles(string folder)
    {
        var indexFile = Path.Combine(folder, Constants.RootDirectoryName, Constants.IndexFileName);
        if (System.IO.File.Exists(indexFile))
        {
            try
            {
                await _locker.WaitAsync();
                using var fs = System.IO.File.OpenRead(indexFile);
                return (await JsonSerializer.DeserializeAsync<List<Core.Entities.File>>(fs)) ?? new();
            }
            finally
            {
                _locker.Release();
            }
        }
        else
        {
            return new();
        }
    }
}
