namespace Pi.FileTransfer.Core.Entities;
public class File
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Extension { get; set; }

    public string RelativePath { get; set; }

    public DateTime LastModified { get; set; }

    public string GetFullPath(Folder folder) => Path.Combine(folder.FullName, RelativePath);
}
