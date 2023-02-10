namespace Pi.FileTransfer.Core.Entities;
public class File
{
    public File(string name, string extension, string relativePath, DateTime lastModified)
        :this(Guid.NewGuid(), name, extension, relativePath, lastModified)
    {
      
    }

    public File(Guid id, string name, string extension, string relativePath, DateTime lastModified)
    {
        Id = Guid.NewGuid();
        Name = name;
        Extension = extension;
        RelativePath = relativePath;
        LastModified = lastModified;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Extension { get; private set; }

    public string RelativePath { get; private set; }

    public DateTime LastModified { get; private set; }

    public string GetFullPath(Folder folder) => Path.Combine(folder.FullName, RelativePath);

    public void UpdateLastModified(DateTime lastModified) => LastModified = lastModified;
}
