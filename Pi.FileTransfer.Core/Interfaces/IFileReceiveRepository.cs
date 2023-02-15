using Pi.FileTransfer.Core.Receives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Interfaces;
public interface IFileReceiveRepository : IRepository
{
    void AddReceipt(Receipt receipt);
    Task<List<Receipt>> GetReceipts(string folder);
}
