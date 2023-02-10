using Pi.FileTransfer.Core.Folders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Interfaces;
public interface IFolderRepository
{
    Task<Folder> GetFolder(string name);
    IAsyncEnumerable<Folder> GetFolders();
    Task Save(Folder folder);
}
