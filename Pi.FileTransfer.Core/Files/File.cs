using Pi.FileTransfer.Core.Folders;

namespace Pi.FileTransfer.Core.Files;
public class File
{
    public File(Folder folder,string name, string extension, string relativePath, DateTime lastModified)
        : this(folder,Guid.NewGuid(), name, extension, relativePath, lastModified)
    {

    }

    public File(Folder folder,Guid id, string name, string extension, string relativePath, DateTime lastModified)
    {
        Folder = folder;
        Id = id;
        Name = name;
        Extension = extension;
        RelativePath = relativePath;
        LastModified = lastModified;
    }

    public Guid Id { get;}

    public Folder Folder { get; }

    public string Name { get;}

    public string Extension { get;}

    public string RelativePath { get;}

    public DateTime LastModified { get; private set; }

    public string GetFullPath() => Path.Combine(Folder.FullName, RelativePath);

    public void UpdateLastModified(DateTime lastModified) => LastModified = lastModified;
}
