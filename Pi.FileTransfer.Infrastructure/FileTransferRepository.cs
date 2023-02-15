using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Folders;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure;
internal class FileTransferRepository : IFileTransferRepository
{
    private readonly FileContext _fileContext;

    public FileTransferRepository(FileContext fileContext)
    {
        _fileContext = fileContext;
    }

    public void AddFailedReceipt(FailedReceipt failedReceipt)
    {
        _fileContext.FailedReceipts.Add(failedReceipt);
    }

    public void AddFailedSegment(FailedSegment failedSegment)
    {
        _fileContext.FailedSegments.Add(failedSegment);
    }

    public void RemoveFailedReceipt(FailedReceipt failedReceipt)
    {
        _fileContext.Remove(failedReceipt);
    }

    public void RemoveFailedSegment(FailedSegment failedSegment)
    {
        _fileContext.Remove(failedSegment);
    }

    public async Task<List<FailedSegment>> GetFailedSegments(Folder folder, Destination destination)
    {
        return await _fileContext
            .FailedSegments
            .Include(x=>x.File)
            .ThenInclude(x=>x.Folder)
            .Include(x=>x.Destination)
            .Where(x => x.File.Folder.Id == folder.Id && x.Destination.Id == destination.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<FailedReceipt>> GetFailedReceipts(Folder folder, Destination destination)
    {
        return await _fileContext
            .FailedReceipts
            .Include(x => x.File)
            .ThenInclude(x => x.Folder)
            .Include(x => x.Destination)
            .Where(x => x.File.Folder.Id == folder.Id && x.Destination.Id == destination.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddOrUpdateLastPosition(LastPosition lastPosition)
    {
        var foundPosition = await _fileContext.LastPositions.FirstOrDefaultAsync(x => x.File.Id == lastPosition.File.Id && x.Destination.Id == lastPosition.Destination.Id);
        if (foundPosition is null)
            _fileContext.LastPositions.Add(lastPosition);
        else
        {
            foundPosition.ReadBytes = lastPosition.ReadBytes;
            _fileContext.Entry(foundPosition).State = EntityState.Modified;
        }
    }

    public async Task ClearLastPosition(Core.Files.File file,Destination destination)
    {
        var foundPosition = await _fileContext.LastPositions.FirstOrDefaultAsync(x => x.File.Id == file.Id && x.Destination.Id == destination.Id);
        if (foundPosition is not null)
            _fileContext.LastPositions.Remove(foundPosition);
    }

    public async Task<LastPosition?> GetLastPosition(Core.Files.File file, Destination destination)
    {
        var foundLastPosition = await _fileContext.LastPositions.AsNoTracking().FirstOrDefaultAsync(x=>x.File.Id == file.Id &&  x.Destination.Id == destination.Id);
        if(foundLastPosition is not null)
            return foundLastPosition;

        return null;
    }

    public IUnitOfWork UnitOfWork => _fileContext;

}
