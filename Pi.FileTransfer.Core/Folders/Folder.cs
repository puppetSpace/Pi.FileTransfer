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

    public Folder(Guid id,string name, string fullName, List<Files.File> files, List<Destination> destinations)
    {
        Id = id;
        Name = name;
        _destinations = destinations ?? new List<Destination>();
        _files = files ?? new List<Files.File>();
        FullName = fullName;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string FullName { get; }


    public IEnumerable<Destination> Destinations => _destinations;

    public IEnumerable<Files.File> Files => _files;

    public void RemoveFile(Files.File file)
    {
        _files.Remove(file);
    }

    public void AddFile(Files.File file, bool withEvent = false)
    {
        _files.Add(file);
        if(withEvent)
            Events.Add(new FileAddedEvent(file, this));
    }

    public void UpdateFile(string path, DateTime lastModified)
    {
        var existing = _files.FirstOrDefault(x => x.GetFullPath() == path);
        if (existing is not null)
        {
            existing.Update(lastModified);
            Events.Add(new FileUpdatedEvent(existing, this));
        }
    }

    public void AddDestination(Destination destination)
    {
        if (Destinations.Any(x => string.Equals(x.Name, destination.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DestinationException($"Destination with name '{destination.Name}' already exists for this folder");

        if (Destinations.Any(x => string.Equals(x.Address, destination.Address, StringComparison.OrdinalIgnoreCase)))
            throw new DestinationException($"Destination with address '{destination.Address}' already exists for this folder");

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

    public static Folder Empty { get; } = new Folder(Guid.Empty,"", "", new(), new());
}
