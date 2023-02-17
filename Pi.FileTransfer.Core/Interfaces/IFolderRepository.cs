using Pi.FileTransfer.Core.Folders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Interfaces;
public interface IFolderRepository : IRepository
{
    Task<Folder?> Get(string name);
    Task<List<Folder>> GetAll();
    void Add(Folder folder);
    void Update(Folder folder);
}
