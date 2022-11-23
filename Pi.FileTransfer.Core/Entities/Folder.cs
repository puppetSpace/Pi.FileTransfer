using Pi.FileTransfer.Core.Commands;
using Pi.FileTransfer.Core.Events;
using Pi.FileTransfer.Core.Exceptions;
using System.Data.SqlTypes;

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
        Events.Add(new FileAddedEvent(entity, this));
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
        }
    }

    public void UpdateFile(string path, DateTime lastModified)
    {
        var existing = _files.FirstOrDefault(x => x.GetFullPath(this) == path);
        if (existing is not null)
        {
            existing.LastModified = lastModified;
            Events.Add(new FileUpdatedEvent(existing, this));
        }
    }

    public void AddDestination(Destination destination)
    {
        if (Destinations.Any(x => string.Equals(x.Name, destination.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DestinationException($"Destination with name '{destination.Name}' already exists for this folder");

        if (Destinations.Any(x => string.Equals(x.Address, destination.Address, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Destination with address '{destination.Address}' already exists for this folder");

        _destinations.Add(destination);

        Events.Add(new DestinationAddedEvent(this, destination));
    }
    public void DeleteDestination(string name)
    {
        if(Destinations.FirstOrDefault(x=>string.Equals(x.Name,name, StringComparison.OrdinalIgnoreCase)) is var destination && destination is not null)
        {
            _destinations.Remove(destination);
        }
    }

    public static Folder Empty { get; } = new Folder("", "", new(), new());
}
