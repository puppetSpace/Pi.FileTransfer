using Microsoft.EntityFrameworkCore;
using Pi.FileTransfer.Core.Interfaces;
using Pi.FileTransfer.Core.Receives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Infrastructure;
internal class FileReceiveRepository : IFileReceiveRepository
{
    private readonly FileContext _fileContext;

    public FileReceiveRepository(FileContext fileContext)
    {
        _fileContext = fileContext;
    }
    public IUnitOfWork UnitOfWork => _fileContext;


    public void AddReceipt(Receipt receipt)
    {
        _fileContext.Receipts.Add(receipt);
    }

    public async Task<List<Receipt>> GetReceipts(string folder)
    {
        return await _fileContext
            .Receipts
            .Where(receipt => receipt.Folder == folder)
            .AsNoTracking()
            .ToListAsync();
    }
}
