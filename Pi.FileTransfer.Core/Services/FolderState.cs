using Pi.FileTransfer.Core.Folders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.FileTransfer.Core.Services;
internal class FolderState
{
    private readonly List<Folder> _folders = new List<Folder>();
    private object _lock = new object();

    public FolderState(List<Folder> initialState)
    {
        _folders.AddRange(initialState);
    }

    public bool Exists(string folderPath)
    {
        lock (_lock)
        {
            return _folders.Any(x => string.Equals(x.FullName, folderPath, StringComparison.OrdinalIgnoreCase));
        }
    }

    public void AddToState(Folder folder)
    {
        lock (_lock)
        {
            if(!_folders.Any(x=>x.Id == folder.Id))
                _folders.Add(folder);
        }
    }
}
