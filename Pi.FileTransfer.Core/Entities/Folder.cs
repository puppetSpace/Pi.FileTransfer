using Pi.FileTransfer.Core.Commands;

namespace Pi.FileTransfer.Core.Entities;
public class Folder : EntityBase
{

    private readonly List<Destination> _destinations;
    private readonly List<Entities.File> _files;

    public Folder(string name, string fullName, List<Entities.File> files, List<Destination> destinations)
    {
        Name = name;
        _destinations = destinations ?? new List<Destination>();
        _files = files ?? new List<Entities.File>();
        FullName = fullName;
    }

    public string Name { get;}

    public string FullName { get; }


    public IEnumerable<Destination> Destinations => _destinations;

    public IEnumerable<Entities.File> Files => _files;

    public void RemoveFile(File file)
    {
        _files.Remove(file);
    }

    public void AddFile(string path, DateTime lastModified)
    {
        var entity = new Entities.File
        {
            Id = Guid.NewGuid(),
            Name = Path.GetFileNameWithoutExtension(path),
            LastModified = lastModified,
            Extension = Path.GetExtension(path),
            RelativePath = path.Replace($"{this.FullName}{Path.DirectorySeparatorChar}", "")
        };
        _files.Add(entity);
        Events.Add(new AddFileCommand(entity, this));
    }

    public void AddFile(File file)
    {
        _files.Add(file);
    }

    public void UpdateFile(File file)
    {
        var existing = _files.FirstOrDefault(x => x.GetFullPath(this) == file.GetFullPath(this));
        if (existing is not null)
        {
            existing.LastModified = file.LastModified;
            Events.Add(new UpdateFileCommand(file, this));
        }
    }

    public void UpdateFile(string path, DateTime lastModified)
    {
        var existing = _files.FirstOrDefault(x => x.GetFullPath(this) == path);
        if (existing is not null)
        {
            existing.LastModified = lastModified;
            Events.Add(new UpdateFileCommand(existing, this));
        }
    }
}
