using Pi.FileTransfer.Core.Folders;

namespace Pi.FileTransfer.Core.Files;
public class File
{
    public File(Folder folder,string name, string extension, string relativePath, DateTime lastModified)
        : this(folder,Guid.NewGuid(), name, extension, relativePath, lastModified, 1)
    {

    }

    public File(Folder folder,Guid id, string name, string extension, string relativePath, DateTime lastModified, int version)
    {
        Folder = folder;
        Id = id;
        Name = name;
        Extension = extension;
        RelativePath = relativePath;
        LastModified = lastModified;
        Version = version;
    }

    public Guid Id { get;}

    public Folder Folder { get; }

    public string Name { get;}

    public string Extension { get;}

    public string RelativePath { get;}

    public DateTime LastModified { get; private set; }

    public int Version { get; private set; }

    public string GetFullPath() => Path.Combine(Folder.FullName, RelativePath);

    public void Update(DateTime lastModified)
    {
        LastModified = lastModified;
        Version++;
    }
}
