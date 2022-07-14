using Pi.FileTransfer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Interfaces;
public interface IFolderRepository
{
    IAsyncEnumerable<Folder> GetFolders();
    Task Save(Folder folder);
}
