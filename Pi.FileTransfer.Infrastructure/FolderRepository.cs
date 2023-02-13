using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Infrastructure;
internal class FolderRepository : IFolderRepository
{
    private readonly string _basePath;
    private readonly FileContext _fileContext;

    public FolderRepository(IOptions<AppSettings> options, FileContext fileContext)
    {
        _basePath = options.Value.BasePath;
        _fileContext = fileContext;
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

        await _fileContext.SaveChangesAsync();
    }

    private async Task SaveFileIndex(Folder folder)
    {
        var existing = await _fileContext
            .Files
            .Where(x => x.Folder == folder.Name)
            .AsNoTracking()
            .ToListAsync();

        var toDelete = existing.Where(x => !folder.Files.Any(y => y.RelativePath == x.RelativePath));
        var toAdd = folder.Files.Where(x => !existing.Any(y => y.RelativePath == x.RelativePath)).Select(x => new DbModels.File() { Id = x.Id, Extension = x.Extension, Folder = folder.Name, LastModified = x.LastModified, Name = x.Name, RelativePath = x.RelativePath });

        _fileContext.Files.RemoveRange(toDelete);
        _fileContext.Files.AddRange(toAdd);
    }

    private async Task SaveDestinations(Folder folder)
    {
        var existing = await _fileContext
            .Destinations
            .Where(x => x.Folder == folder.Name)
            .AsNoTracking()
            .ToListAsync();

        var toDelete = existing.Where(x => !folder.Files.Any(y => y.Name == x.Name));
        var toAdd = folder.Destinations.Where(x => !existing.Any(y => y.Name == x.Name)).Select(x => new DbModels.Destination() { Name = x.Name, Folder = folder.Name, Address = x.Address });

        _fileContext.Destinations.RemoveRange(toDelete);
        _fileContext.Destinations.AddRange(toAdd);
    }

    private async Task<Folder> BuildFolderEntityAsync(string folder)
    {
        var folderName = Path.GetFileName(folder);

        return new(folderName, Path.Combine(_basePath, folderName), await GetFiles(folder), await GetDestinations(folder));
    }

    private async Task<List<Core.Files.File>> GetFiles(string folder)
    {
        return await _fileContext
            .Files
            .Where(x => x.Folder == folder)
            .Select(x => new Core.Files.File(x.Name, x.Extension, x.RelativePath, x.LastModified))
            .AsNoTracking()
            .ToListAsync();
    }

    private async Task<List<Destination>> GetDestinations(string folder)
    {
        return await _fileContext
             .Destinations
             .Where(x => x.Folder == folder)
             .Select(x => new Destination(x.Name, x.Address))
             .AsNoTracking()
             .ToListAsync();
    }
}
