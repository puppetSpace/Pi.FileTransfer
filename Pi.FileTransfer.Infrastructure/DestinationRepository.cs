using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure;
internal class DestinationRepository : IDestinationRepository
{
    private readonly FileContext _fileContext;

    public DestinationRepository(FileContext fileContext)
    {
        _fileContext = fileContext;
    }

    public IUnitOfWork UnitOfWork => _fileContext;

    public void Add(Destination destination)
    {
        _fileContext.Destinations.Add(destination);
    }

    public async Task Delete(Guid destinationId)
    {
        var found = await _fileContext.Destinations.FirstOrDefaultAsync(x=>x.Id == destinationId);
        if (found is not null)
        {
            _fileContext.Destinations.Remove(found);
            _fileContext.FailedReceipts.RemoveRange(await _fileContext.FailedReceipts.Where(x => x.Destination.Id == destinationId).ToListAsync());
            _fileContext.FailedSegments.RemoveRange(await _fileContext.FailedSegments.Where(x=>x.Destination.Id == destinationId).ToListAsync());
        }
    }

    public async Task<Destination?> Get(Guid id)
    {
        return await _fileContext.Destinations.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Destination>> GetAll()
    {
        return await _fileContext.Destinations.AsNoTracking().ToListAsync();
    }
}
