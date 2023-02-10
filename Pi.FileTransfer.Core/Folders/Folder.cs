using Pi.FileTransfer.Core.Common;
using Pi.FileTransfer.Core.Destinations;
using Pi.FileTransfer.Core.Destinations.Events;
using Pi.FileTransfer.Core.Destinations.Exceptions;
using Pi.FileTransfer.Core.Files.Events;

namespace Pi.FileTransfer.Core.Folders;
public class Folder : EntityBase
{

    private readonly List<Destination> _destinations;
    private readonly List<Files.File> _files;

    public Folder(string name, string fullName, List<Files.File> files, List<Destination> destinations)
    {
        Name = name;
        _destinations = destinations ?? new List<Destination>();
        _files = files ?? new List<Files.File>();
        FullName = fullName;
    }

    public string Name { get; }

    public string FullName { get; }


    public IEnumerable<Destination> Destinations => _destinations;

    public IEnumerable<Files.File> Files => _files;

    public void RemoveFile(Files.File file)
    {
        _files.Remove(file);
    }

    public void AddFile(string path, DateTime lastModified)
    {
        var entity = new Files.File(Path.GetFileNameWithoutExtension(path)
            , Path.GetExtension(path)
            , path.Replace($"{FullName}{Path.DirectorySeparatorChar}", "")
            , lastModified);

        _files.Add(entity);
        Events.Add(new FileAddedEvent(entity, this));
    }

    public void AddFile(Files.File file)
    {
        _files.Add(file);
    }

    public void UpdateFile(Files.File file)
    {
        var existing = _files.FirstOrDefault(x => x.GetFullPath(this) == file.GetFullPath(this));
        if (existing is not null)
        {
            existing.UpdateLastModified(file.LastModified);
        }
    }

    public void UpdateFile(string path, DateTime lastModified)
    {
        var existing = _files.FirstOrDefault(x => x.GetFullPath(this) == path);
        if (existing is not null)
        {
            existing.UpdateLastModified(lastModified);
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
        if (Destinations.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)) is var destination && destination is not null)
        {
            _destinations.Remove(destination);
        }
    }

    public static Folder Empty { get; } = new Folder("", "", new(), new());
}
