using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Folders.Events;
using Pi.FileTransfer.Core.Interfaces;

namespace Pi.FileTransfer.Infrastructure;
internal class FolderRepository : IFolderRepository
{
    private readonly FileContext _fileContext;

    public FolderRepository(FileContext fileContext)
    {
        _fileContext = fileContext;
    }

    public IUnitOfWork UnitOfWork => _fileContext;


    public async Task<List<Folder>> GetFolders()
    {
        return await _fileContext
            .Folders
            .Include(x => x.Destinations)
            .Include(x => x.Files)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Folder?> GetFolder(string name)
    {
        return await _fileContext
            .Folders
            .Where(x => x.Name == name)
            .FirstOrDefaultAsync();
    }

    public void Add(Folder folder)
    {
        folder.Events.Add(new FolderAddedEvent(folder));
        _fileContext.Folders.Add(folder);
    }

    public void Update(Folder folder)
    {
        _fileContext.Entry(folder).State = EntityState.Modified;
    }



}
